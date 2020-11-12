using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public string musicTitle = "bpm_rt";
    public bool debug;

    [SerializeField, HeaderAttribute("Game Objects")] GameObject laneGameObject;
    [SerializeField] GameObject noteGameObject;
    [SerializeField] GameObject longNoteGameObject;
    [SerializeField] GameObject laneMoverGameObject;
    [SerializeField] GameObject gameCameraGameObject;

    [SerializeField, HeaderAttribute("Materials")] Material materialBlue;
    [SerializeField] Material materialYellow;

    [SerializeField, HeaderAttribute("Note Sprites")] Sprite[] lanelatterSprites;
    [SerializeField] Sprite[] tapnoteSprites;
    [SerializeField] Sprite[] slidenoteSprites;

    [SerializeField, HeaderAttribute("TimingSupport Sprites")] Sprite tapnoteTimingSupport;
    [SerializeField] Sprite slidenoteTimingSupport;
    [SerializeField] Sprite tapnoteTimingMultiSupport;
    [SerializeField] Sprite slidenoteTimingMultiSupport;

    [SerializeField] AnimationCurve[] curves;
    
    [SerializeField, HeaderAttribute("Times")] public long gameStartedTime;
    public static long gameMasterTime;

    public static Dictionary<char, long> gameMasterPositions = new Dictionary<char, long>();
    public static List<LongNoteInfo> longNoteInfoStorage = new List<LongNoteInfo>();

    int noteNum;
    int laneMoverNum;

    string allLaneIDString;

    Dictionary<char, sbyte> keyNumsOfLanes = new Dictionary<char, sbyte>();
    Dictionary<char, GameObject> lanesDictionary = new Dictionary<char, GameObject>();
    Dictionary<string, string> scoreTextData = new Dictionary<string, string>();

    Dictionary<char, TimingPoints> timingPtsDic = new Dictionary<char, TimingPoints>();

    class TimingPoints {
        
        public class TimingPoint {
            public long ticks;
            public long position;
            public double beat;
            public double bpm;
            public double scroll;

            public TimingPoint(long ticks, long position, double beat, double bpm, double scroll) {
                this.ticks = ticks;
                this.position = position;
                this.beat = beat;
                this.bpm = bpm;
                this.scroll = scroll;
            }

            public long GetHitTickByBeat(double beat) {
                return (long)(this.ticks + (beat - this.beat) * (600000000 / this.bpm));
            }

            public long GetPositionTickByBeat(double beat) {
                return (long)(this.position + (beat - this.beat) * this.scroll * 10000000);
            }

            public long GetPositionTickByTime(double time) {
                return (long)(this.position + (time - this.ticks) / (600000000 / this.bpm) * this.scroll * 10000000);
            }
        }

        public List<TimingPoint> timingPoints = new List<TimingPoint>();

        public TimingPoints(double bpm, double scroll) {
            timingPoints.Add(new TimingPoint(0, 0, 0, bpm, scroll));
        }

        public void AddBPMPoint(double beat, double bpm) {
            TimingPoint lastTP = timingPoints[timingPoints.Count - 1];
            timingPoints.Add(new TimingPoint(lastTP.GetHitTickByBeat(beat), lastTP.GetPositionTickByBeat(beat), beat, bpm, lastTP.scroll));
        }

        public void AddScrollPoint(double beat, double scroll) {
            TimingPoint lastTP = timingPoints[timingPoints.Count - 1];
            timingPoints.Add(new TimingPoint(lastTP.GetHitTickByBeat(beat), lastTP.GetPositionTickByBeat(beat), beat, lastTP.bpm, scroll));
        }

        public void AddJumpPoint(double beat, double jump) {
            TimingPoint lastTP = timingPoints[timingPoints.Count - 1];
            timingPoints.Add(new TimingPoint(lastTP.GetHitTickByBeat(beat), lastTP.GetPositionTickByBeat(beat + jump), beat, lastTP.bpm, lastTP.scroll));
        }

        public long GetHitTickByBeat(double beat) {
            int index = timingPoints.Count - 1;
            while (0 < index && beat <= timingPoints[index].beat) index--;
            return timingPoints[index].GetHitTickByBeat(beat);
        }

        public long GetPositionTickByBeat(double beat) {
            int index = timingPoints.Count - 1;
            while (0 < index && beat <= timingPoints[index].beat) index--;
            return timingPoints[index].GetPositionTickByBeat(beat);
        }

        public long GetPositionTickByTime(long time) {
            int index = timingPoints.Count - 1;
            while (0 < index && time <= timingPoints[index].ticks) index--;
            return timingPoints[index].GetPositionTickByTime(time);
        }
    }

    class NoteDataHolder {
        public char type;
        public char lane;
        public double beat;
        public short longNoteID;
        public double speed;
        public AnimationCurve[] curve;
        public bool isReversed;
        public float positionNotesFrom;

        public NoteDataHolder(char type, char lane, double beat, short longNoteID, double speed, AnimationCurve[] curve, bool isReversed, float positionNotesFrom) {
            this.type = type;
            this.lane = lane;
            this.beat = beat;
            this.longNoteID = longNoteID;
            this.speed = speed;
            this.curve = curve;
            this.isReversed = isReversed;
            this.positionNotesFrom = positionNotesFrom;
        }
    }

    class LaneMoverDataHolder {
        public string type;
        public char lane;
        public double beat;
        public double speed;
        public AnimationCurve[] curve;
        public float fromValue;
        public float toValue;

        public LaneMoverDataHolder(string type, char lane, double beat, double speed, AnimationCurve[] curve, float fromValue, float toValue) {
            this.type = type;
            this.lane = lane;
            this.beat = beat;
            this.speed = speed;
            this.curve = curve;
            this.fromValue = fromValue;
            this.toValue = toValue;
        }
    }

    public class LongNoteInfo {
        public float startPosition = 0;
        public float startTime = -1;
        
        public float endPosition = 0;
        public float endTime = -1;

        public byte alpha = 0;

        public LongNoteInfo() {

        }
    }

    AnimationCurve ConvertSvgPathToAnimationCurve(string path) {
        var splittedPath = path.Split(' ');
        var keyframes = new List<Keyframe>();

        float time = 0,
              value = 0,
              inTangent = 0,
              outTangent = 0,
              inWeight = 0,
              outWeight = 0,
              lastTime = 0,
              lastValue = 0;
        sbyte mode = 0;
        sbyte valuesCtr = 0;

        foreach (var i in splittedPath) {
            switch (i) {
                case "M": case "L": mode = 1; break;
                case "m": case "l": mode = 2; break;
                case "C": {
                    mode = 3; 
                    valuesCtr = 0;
                    inTangent = 0;
                    outTangent = 0;
                    inWeight = 0;
                    outWeight = 0;
                } break;
                case "c": {
                    mode = 4; 
                    valuesCtr = 0;
                    inTangent = 0;
                    outTangent = 0;
                    inWeight = 0;
                    outWeight = 0;
                } break;
                case "H": mode = 5; break;
                case "h": mode = 6; break;
                case "V": mode = 7; break;
                case "v": mode = 8; break;
                default: {
                    switch (mode) {
                        case 1: {
                            time = float.Parse(i.Split(',')[0]);
                            value = float.Parse(i.Split(',')[1]);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                        case 2: {
                            time += float.Parse(i.Split(',')[0]);
                            value += float.Parse(i.Split(',')[1]);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                        case 3: {
                            switch (valuesCtr % 3) {
                                case 0: {
                                    outWeight = float.Parse(i.Split(',')[0]) - time;
                                    if (outWeight == 0) outWeight = 0.000001f;
                                    outTangent = (value - float.Parse(i.Split(',')[1])) / outWeight;
                                } break;
                                case 1: {
                                    inWeight = float.Parse(i.Split(',')[0]);
                                    inTangent = float.Parse(i.Split(',')[1]);
                                } break;
                                case 2: {
                                    lastTime = time;
                                    lastValue = value;
                                    time = float.Parse(i.Split(',')[0]);
                                    value = float.Parse(i.Split(',')[1]);

                                    inWeight = time - inWeight;
                                    if (inWeight == 0) inWeight = 1 / 1E10f;
                                    inTangent = (inTangent - value) / inWeight;

                                    keyframes.Add(new Keyframe(lastTime / 100, 1 - lastValue / 100, 0, outTangent, 0, outWeight / (time - lastTime)));
                                    keyframes.Add(new Keyframe(time / 100, 1 - value / 100, inTangent, 0, inWeight / (time - lastTime), 0));
                                } break;
                            }
                            valuesCtr++;
                        } break;
                        case 4: {
                            switch (valuesCtr % 3) {
                                case 0: {
                                    outWeight = float.Parse(i.Split(',')[0]);
                                    if (outWeight == 0) outWeight = 0.000001f;
                                    outTangent = -float.Parse(i.Split(',')[1]) / outWeight;
                                } break;
                                case 1: {
                                    inWeight = float.Parse(i.Split(',')[0]);
                                    inTangent = float.Parse(i.Split(',')[1]);
                                } break;
                                case 2: {
                                    lastTime = time;
                                    lastValue = value;
                                    time += float.Parse(i.Split(',')[0]);
                                    value += float.Parse(i.Split(',')[1]);

                                    inWeight = (time - lastTime) - inWeight;
                                    if (inWeight == 0) inWeight = 1 / 1E10f;
                                    inTangent = (inTangent - (value - lastValue)) / inWeight;

                                    keyframes.Add(new Keyframe(lastTime / 100, 1 - lastValue / 100, 0, outTangent, 0, outWeight / (time - lastTime)));
                                    keyframes.Add(new Keyframe(time / 100, 1 - value / 100, inTangent, 0, inWeight / (time - lastTime), 0));
                                } break;
                            }
                            valuesCtr++;
                        } break;
                        case 5: {
                            time = float.Parse(i);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                        case 6: {
                            time += float.Parse(i);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                        case 7: {
                            value = float.Parse(i);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                        case 8: {
                            value += float.Parse(i);
                            keyframes.Add(new Keyframe(time / 100, 1 - value / 100, 0, 0, 0, 0));
                        } break;
                    }
                } break;
            }
        }

        return new AnimationCurve(keyframes.ToArray());
    }

    void CreateLane(char lane, float xpos, float ypos, float direction, float alpha, sbyte keyNum) {
        
        GameObject tempLane = Instantiate(laneGameObject);

        allLaneIDString += lane;
        gameMasterPositions.Add(lane, 0);
        timingPtsDic.Add(lane, new TimingPoints(double.Parse(scoreTextData["bpm"]), 1d));
        lanesDictionary.Add(lane, tempLane);
        keyNumsOfLanes.Add(lane, keyNum);

        tempLane.transform.parent = this.gameObject.transform;

        tempLane.name = "lane_" + lane;
        tempLane.transform.position = new Vector3(xpos, 0f, ypos);
        tempLane.transform.eulerAngles = new Vector3(90f, 0f, direction);

        tempLane.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = lanelatterSprites[keyNum];
    }
    
    void CreateNote(char type, char lane, AnimationCurve[] curve, long hitTick, long appearPosition, long hitPosition, short longNoteID, bool isReversed, bool isMultiNote, float positionNotesFrom) {
        
        GameObject tempNote;

        if (type == '1' || type == '2') {
            tempNote = Instantiate(noteGameObject);

            tempNote.transform.parent = lanesDictionary[lane].transform;
            tempNote.name = "note_" + noteNum++;

            SpriteRenderer tempSRNote = tempNote.gameObject.GetComponent<SpriteRenderer>();
            if (type == '1') tempSRNote.sprite = tapnoteSprites[keyNumsOfLanes[lane]];
            else tempSRNote.sprite = slidenoteSprites[keyNumsOfLanes[lane]];

        } else {
            tempNote = Instantiate(longNoteGameObject);
            tempNote.transform.parent = lanesDictionary[lane].transform;
            tempNote.name = "note_" + noteNum++;

            MeshRenderer tempMR = tempNote.gameObject.GetComponent<MeshRenderer>();
            if (type == '3') tempMR.materials[0] = materialBlue;
            else tempMR.materials[0] = materialYellow;
            tempMR.material.color -= new Color(0, 0, 0, 0.7f);

            LongNoteProcesser tempLNP = tempNote.gameObject.GetComponent<LongNoteProcesser>();
            tempLNP.longNoteID = longNoteID;
            tempLNP.isReversed = isReversed;
        }

        SpriteRenderer tempSRTimingSupport = tempNote.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (isMultiNote) {
            if (type == '1' || type == '3') tempSRTimingSupport.sprite = tapnoteTimingMultiSupport;
            else tempSRTimingSupport.sprite = slidenoteTimingMultiSupport;
        } else {
            if (type == '1' || type == '3') tempSRTimingSupport.sprite = tapnoteTimingSupport;
            else tempSRTimingSupport.sprite = slidenoteTimingSupport;
        }

        NoteProcesser tempNoteProcesser = tempNote.gameObject.GetComponent<NoteProcesser>();

        tempNoteProcesser.type = type;
        tempNoteProcesser.lane = lane;
        tempNoteProcesser.keyNum = keyNumsOfLanes[lane];
        tempNoteProcesser.curve = curve;
        tempNoteProcesser.hitTick = hitTick;
        tempNoteProcesser.appearPosition = appearPosition;
        tempNoteProcesser.hitPosition = hitPosition;
        tempNoteProcesser.longNoteID = longNoteID;
        tempNoteProcesser.isReversed = isReversed;
        tempNoteProcesser.isMultiNote = isMultiNote;
        tempNoteProcesser.positionNotesFrom = positionNotesFrom;
    }

    void CreateLaneMover(string type, char lane, AnimationCurve[] curve, long hitTick, long appearPosition, long hitPosition, float fromValue, float toValue) {
        GameObject tempLaneMover = Instantiate(laneMoverGameObject);

        if (type[0] == 'c') {
            type = type.Substring(1);
        }
        else tempLaneMover.transform.parent = lanesDictionary[lane].transform;
        tempLaneMover.name = "laneMover_" + laneMoverNum++;

        var tempLaneMoverProcesser = tempLaneMover.gameObject.GetComponent<LaneMoverProcesser>();

        tempLaneMoverProcesser.type = type;
        tempLaneMoverProcesser.lane = lane;
        tempLaneMoverProcesser.curve = curve;
        tempLaneMoverProcesser.hitTick = hitTick;
        tempLaneMoverProcesser.appearPosition = appearPosition;
        tempLaneMoverProcesser.hitPosition = hitPosition;
        tempLaneMoverProcesser.fromValue = fromValue;
        tempLaneMoverProcesser.toValue = toValue;
    }

    void LoadScore(string scoreFileName) {

        gameMasterPositions = new Dictionary<char, long>();
        keyNumsOfLanes = new Dictionary<char, sbyte>();
        lanesDictionary = new Dictionary<char, GameObject>();
        scoreTextData = new Dictionary<string, string>();
        timingPtsDic = new Dictionary<char, TimingPoints>();
        noteNum = 0;
        laneMoverNum = 0;
        allLaneIDString = "";

        string[] deleteIgnore = new string[] {
            "Info"
        };

        foreach (Transform child in gameObject.transform) {
            if (Array.IndexOf(deleteIgnore, child.name) > -1) continue;
            GameObject.Destroy(child.gameObject);
        }
        
        string scoreFullText = ((TextAsset)Resources.Load("scores/" + scoreFileName + "/" + scoreFileName + ".qwertyuscore")).text;

        scoreFullText = Regex.Replace(scoreFullText, @"\/\*.*\*\/", "", RegexOptions.Singleline);
        scoreFullText = Regex.Replace(scoreFullText, @"//.*?$", "", RegexOptions.Multiline);

        scoreTextData["title"] = Regex.Matches(scoreFullText, @"title:(.*)")[0].Groups[1].Value;
        scoreTextData["author"] = Regex.Matches(scoreFullText, @"author:(.*)")[0].Groups[1].Value;
        scoreTextData["bgm"] = Regex.Matches(scoreFullText, @"bgm:(.*)")[0].Groups[1].Value;
        scoreTextData["bgmvol"] = Regex.Matches(scoreFullText, @"bgmvol:(.*)")[0].Groups[1].Value;
        scoreTextData["bpm"] = Regex.Matches(scoreFullText, @"bpm:(.*)")[0].Groups[1].Value;
        scoreTextData["offset"] = Regex.Matches(scoreFullText, @"offset:(.*)")[0].Groups[1].Value;

        scoreTextData["path"] = Regex.Matches(scoreFullText, @"path:(.*?)score:", RegexOptions.Singleline)[0].Groups[1].Value;
        scoreTextData["score"] = Regex.Matches(scoreFullText, @"score:(.*)", RegexOptions.Singleline)[0].Groups[1].Value;

        CreateLane('1', -120, -70, 0, 1, 1);
        CreateLane('2', -80, -70, 0, 1, 2);
        CreateLane('3', -40, -70, 0, 1, 3);
        CreateLane('4', 0, -70, 0, 1, 4);
        CreateLane('5', 40, -70, 0, 1, 5);
        CreateLane('6', 80, -70, 0, 1, 6);
        CreateLane('7', 120, -70, 0, 1, 7);

        //creating main camera
        GameObject tempGameCamera = Instantiate(gameCameraGameObject);
        timingPtsDic.Add('@', new TimingPoints(double.Parse(scoreTextData["bpm"]), 1d));
        lanesDictionary.Add('@', tempGameCamera);
        tempGameCamera.name = "GameCamera";
        tempGameCamera.transform.parent = gameObject.transform;
        tempGameCamera.transform.localPosition = new Vector3(0, 200, -20);
        tempGameCamera.transform.rotation = Quaternion.Euler(85, 0, 0);

        var curvesList = new List<AnimationCurve>();
        var indexOfpathName = new Dictionary<string, int>();
        foreach (var pathString in scoreTextData["path"].Split('\n')) {
            if (!String.IsNullOrWhiteSpace(pathString)) {
                var position = pathString.IndexOf(' ');
                indexOfpathName[pathString.Substring(0, position)] = curvesList.Count;
                curvesList.Add(ConvertSvgPathToAnimationCurve(pathString.Substring(position + 1)));
            }
        }
        curves = curvesList.ToArray();

        var longNoteIDs = new List<string>();
        var noteDataHolders = new List<NoteDataHolder>();
        var laneMoverDataHolders = new List<LaneMoverDataHolder>();

        AnimationCurve[] curve = new AnimationCurve[] {curves[0]};
        double speed = 4;
        bool isReversed = false;
        float positionNotesFrom = 160;
        double beatDefault = 0;
        double startingPoint = 0;

        foreach (Match individualMatch in Regex.Matches(scoreTextData["score"], @"\( *(.*?) *\)", RegexOptions.Singleline)) {
            
            string[] scoreArgs = individualMatch.Groups[1].Value.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            if (scoreArgs.Length > 1 && scoreArgs[1][0] == '~') {
                string temp = allLaneIDString;
                foreach (char lane in scoreArgs[1].Substring(1)) {
                    temp = temp.Replace(lane.ToString(), "");
                }
                scoreArgs[1] = temp;
            }

            switch (scoreArgs[0]) {
                case "1": case "2": case "3": case "4": {
                    foreach (char lane in scoreArgs[1]) {
                        if (scoreArgs.Length >= 4 && longNoteIDs.IndexOf(scoreArgs[3] + lane) == -1) {
                            longNoteIDs.Add(scoreArgs[3] + lane);
                            longNoteInfoStorage.Add(new LongNoteInfo());
                        }
                        noteDataHolders.Add(new NoteDataHolder(
                            scoreArgs[0][0],
                            lane,
                            beatDefault + double.Parse(scoreArgs[2]),
                            (short)(scoreArgs.Length >= 4 ? longNoteIDs.IndexOf(scoreArgs[3] + lane) : -1),
                            speed,
                            curve,
                            isReversed,
                            positionNotesFrom
                        ));
                    }
                } break;
                case "a": case "~a":
                case "x": case "y": case "z": case "~x": case "~y": case "~z":
                case "dx": case "dy": case "dz": case "~dx": case "~dy": case "~dz": {
                    foreach (char lane in scoreArgs[1]) {
                        laneMoverDataHolders.Add(new LaneMoverDataHolder(
                            scoreArgs[0],
                            lane,
                            beatDefault + double.Parse(scoreArgs[2]),
                            speed,
                            curve,
                            scoreArgs.Length < 5 ? 0 : float.Parse(scoreArgs[3]),
                            scoreArgs.Length < 5 ? float.Parse(scoreArgs[3]) : float.Parse(scoreArgs[4])
                        ));
                    }
                } break;
                case "positionfrom": case "posfrom": case "pf": {
                    positionNotesFrom = float.Parse(scoreArgs[1]);
                } break;
                case "beatdefault": case "beatdef": case "bd": {
                    beatDefault = scoreArgs.Length == 1 ? 0 : double.Parse(scoreArgs[1]);
                } break;
                case ">": {
                    startingPoint = scoreArgs.Length == 1 ? beatDefault : beatDefault + double.Parse(scoreArgs[1]);
                } break;
                case "path": case "p": {
                    if (scoreArgs[1][0] == '-') {
                        isReversed = true;
                        scoreArgs[1] = scoreArgs[1].Substring(1);
                    } else isReversed = false;
                    var tempCurves = new List<AnimationCurve>();
                    foreach (var pathName in scoreArgs[1].Split(',')) {
                        tempCurves.Add(curves[indexOfpathName[pathName]]);
                    }
                    curve = tempCurves.ToArray();
                } break;
                case "speed": case "sp": {
                    speed = double.Parse(scoreArgs[1]);
                } break;
                case "bpm": {
                    foreach (char lane in allLaneIDString) {
                        timingPtsDic[lane].AddBPMPoint(beatDefault + double.Parse(scoreArgs[1]), double.Parse(scoreArgs[2]));
                    }
                } break;
                case "scroll": case "scr": {
                    foreach (char lane in scoreArgs[1]) {
                        timingPtsDic[lane].AddScrollPoint(beatDefault + double.Parse(scoreArgs[2]), double.Parse(scoreArgs[3]));
                    }
                } break;
                case "jump": case "jmp": {
                    //if (scoreArgs.Length < 3) throw new SyntaxErrorException("Not enough arguments given (3 arguments required)");
                    foreach (char lane in scoreArgs[1]) {
                        timingPtsDic[lane].AddJumpPoint(beatDefault + double.Parse(scoreArgs[2]), double.Parse(scoreArgs[3]));
                    }
                } break;
            }
        }

        noteDataHolders.Sort((x, y) => x.beat > y.beat ? 1 : x.beat < y.beat ? -1 : 0);
        laneMoverDataHolders.Sort((x, y) => x.beat > y.beat ? 1 : x.beat < y.beat ? -1 : 0);

        for (int index = 0; index < noteDataHolders.Count; index++) {
            NoteDataHolder item = noteDataHolders[index];
            CreateNote(
                item.type,
                item.lane,
                item.curve,
                timingPtsDic[item.lane].GetHitTickByBeat(item.beat),
                timingPtsDic[item.lane].GetPositionTickByBeat(item.beat) - (long)(item.speed * 10000000),
                timingPtsDic[item.lane].GetPositionTickByBeat(item.beat),
                item.longNoteID,
                item.isReversed,
                (index > 0 && item.beat == noteDataHolders[index - 1].beat) || (index < noteDataHolders.Count - 1 && item.beat == noteDataHolders[index + 1].beat),
                item.positionNotesFrom
            );
        }

        foreach (var item in laneMoverDataHolders) {
            CreateLaneMover(
                item.type,
                item.lane,
                item.curve,
                timingPtsDic[item.lane].GetHitTickByBeat(item.beat),
                timingPtsDic[item.lane].GetPositionTickByBeat(item.beat) - (long)(item.speed * 10000000),
                timingPtsDic[item.lane].GetPositionTickByBeat(item.beat),
                item.fromValue,
                item.toValue
            );
        }

        InfoProcesser infoProc = transform.Find("Info").GetComponent<InfoProcesser>();

        infoProc.progress = 0;
        infoProc.combo = 0;
        infoProc.title = scoreTextData["title"];
        infoProc.author = scoreTextData["author"];
        infoProc.score = 0;
        infoProc.fullCombo = true;
        infoProc.allPerfect = true;
        
        gameStartedTime = DateTime.Now.Ticks + 20000000 + (long)(float.Parse(scoreTextData["offset"]) * 10000) - (debug ? timingPtsDic['@'].GetHitTickByBeat(startingPoint) : 0);
        gameMasterTime = DateTime.Now.Ticks - gameStartedTime;
    }

    void Start() {
        LoadScore(musicTitle);
    }

    void Update() {
        if (Input.GetKey("space")) LoadScore(musicTitle);
        gameMasterTime = DateTime.Now.Ticks - gameStartedTime;
        foreach (char lane in allLaneIDString + '@') {
            lanesDictionary[lane].GetComponent<LaneProcesser>().position = timingPtsDic[lane].GetPositionTickByTime(gameMasterTime);
        }
        if (Input.GetKey(KeyCode.Escape)) UnityEngine.Application.Quit();
    }
}