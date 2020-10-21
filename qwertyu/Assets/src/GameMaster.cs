using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    [SerializeField, HeaderAttribute("Game Objects")] GameObject laneGameObject;
    [SerializeField] GameObject noteGameObject;

    [SerializeField, HeaderAttribute("Note Sprites")] Sprite[] lanelatterSprites;
    [SerializeField] Sprite[] tapnoteSprites;
    [SerializeField] Sprite[] slidenoteSprites;

    [SerializeField, HeaderAttribute("TimingSupport Sprites")] Sprite tapnoteTimingSupport;
    [SerializeField] Sprite slidenoteTimingSupport;
    [SerializeField] Sprite tapnoteTimingMultiSupport;
    [SerializeField] Sprite slidenoteTimingMultiSupport;

    [SerializeField] AnimationCurve[] curves;

    long gameStartedTime;
    public static long gameMasterTime;
    public static long gameMasterPosition;

    MusicPlayer musicPlayer;

    int noteNum;
    Dictionary<char, sbyte> keyNumsOfLanes = new Dictionary<char, sbyte>();
    Dictionary<char, GameObject> lanesDictionary = new Dictionary<char, GameObject>();
    Dictionary<string, string> scoreTextData = new Dictionary<string, string>();

    TimingPoints timingPts;

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

        lanesDictionary.Add(lane, tempLane);
        keyNumsOfLanes.Add(lane, keyNum);

        tempLane.transform.parent = this.gameObject.transform;

        tempLane.name = "lane_" + lane;
        tempLane.transform.position = new Vector3(xpos, 0f, ypos);
        tempLane.transform.eulerAngles = new Vector3(90f, 0f, direction);

        tempLane.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = lanelatterSprites[keyNum];
    }
    
    void CreateNote(char type, char lane, AnimationCurve curve, long hit, long appearPosition, long hitPosition, short longNoteID, bool isReversed, bool isMultiNote) {

        GameObject tempNote = Instantiate(noteGameObject);

        tempNote.transform.parent = lanesDictionary[lane].transform;
        tempNote.name = "note_" + noteNum++;

        if (type == '1' || type == '3') tempNote.gameObject.GetComponent<SpriteRenderer>().sprite = tapnoteSprites[keyNumsOfLanes[lane]];
        else tempNote.gameObject.GetComponent<SpriteRenderer>().sprite = slidenoteSprites[keyNumsOfLanes[lane]];

        if (isMultiNote) {
            if (type == '1') tempNote.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = tapnoteTimingMultiSupport;
            else if (type == '2') tempNote.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = slidenoteTimingMultiSupport;
        } else {
            if (type == '1') tempNote.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = tapnoteTimingSupport;
            else if (type == '2') tempNote.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = slidenoteTimingSupport;
        }

        var tempNoteManager = tempNote.gameObject.GetComponent<NoteProcesser>();

        tempNoteManager.type = type;
        tempNoteManager.lane = lane;
        tempNoteManager.keyNum = keyNumsOfLanes[lane]; //Additional
        tempNoteManager.curve = curve;
        tempNoteManager.hit = hit;
        tempNoteManager.appearPosition = appearPosition;
        tempNoteManager.hitPosition = hitPosition;
        tempNoteManager.longNoteID = longNoteID;
        tempNoteManager.isReversed = isReversed;
        tempNoteManager.isMultiNote = isMultiNote;
    }

    void LoadScore(string scoreFileName) {
        
        string scoreFullText = ((TextAsset)Resources.Load("scores/" + scoreFileName + "/" + scoreFileName + ".qwertyuscore")).text;

        scoreFullText = Regex.Replace(scoreFullText, @"^//.*?$", "", RegexOptions.Multiline);

        scoreTextData["title"] = Regex.Matches(scoreFullText, @"title:(.*)")[0].Groups[1].Value;
        scoreTextData["author"] = Regex.Matches(scoreFullText, @"author:(.*)")[0].Groups[1].Value;
        scoreTextData["bgm"] = Regex.Matches(scoreFullText, @"bgm:(.*)")[0].Groups[1].Value;
        scoreTextData["bgmvol"] = Regex.Matches(scoreFullText, @"bgmvol:(.*)")[0].Groups[1].Value;
        scoreTextData["bpm"] = Regex.Matches(scoreFullText, @"bpm:(.*)")[0].Groups[1].Value;
        scoreTextData["scroll"] = Regex.Matches(scoreFullText, @"scroll:(.*)")[0].Groups[1].Value;
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

        musicPlayer = new MusicPlayer(scoreTextData["bgm"]);

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

        timingPts = new TimingPoints(double.Parse(scoreTextData["bpm"]), double.Parse(scoreTextData["scroll"]));

        var longNoteIDs = new List<string>();
        var primaryProcessedScore = new List<object[]>();
        double speed = 4;
        int path = 0;
        bool isReversed = false;

        foreach (Match individualMatch in Regex.Matches(scoreTextData["score"], @"\( *(.*?) *\)", RegexOptions.Singleline)) {
            string[] scoreArgs = individualMatch.Groups[1].Value.Split(' ');
            switch (scoreArgs[0]) {
                case "1": case "2": case "3": case "4": {
                    foreach (char lane in scoreArgs[1]) {
                        if (scoreArgs.Length >= 4 && longNoteIDs.IndexOf(scoreArgs[3] + lane) == -1) longNoteIDs.Add(scoreArgs[3] + lane);
                        primaryProcessedScore.Add(new object[] {
                            1,
                            scoreArgs[0][0],
                            lane,
                            path,
                            speed,
                            double.Parse(scoreArgs[2]),
                            scoreArgs.Length >= 4 ? longNoteIDs.IndexOf(scoreArgs[3] + lane) : -1,
                            isReversed,
                            scoreArgs[1].Length >= 2
                        });
                    }
                } break;
                case "a": case "d": case "x": case "y": {
                    foreach (char lane in scoreArgs[1]) {
                        primaryProcessedScore.Add(new object[] {
                            2,
                            scoreArgs[0][0],
                            lane,
                            path,
                            speed,
                            double.Parse(scoreArgs[2]),
                            float.Parse(scoreArgs[3]),
                            float.Parse(scoreArgs[4])
                        });
                    }
                } break;
                case "path": {
                    isReversed = scoreArgs[1][0] == '-';
                    path = indexOfpathName[isReversed ? scoreArgs[1].Substring(1) : scoreArgs[1]];
                } break;
                case "speed": {
                    speed = double.Parse(scoreArgs[1]);
                } break;
                case "bpm": {
                    timingPts.AddBPMPoint(double.Parse(scoreArgs[1]), double.Parse(scoreArgs[2]));
                } break;
                case "scroll": {
                    timingPts.AddScrollPoint(double.Parse(scoreArgs[2]), double.Parse(scoreArgs[3]));
                } break;
                case "jump": {
                    timingPts.AddJumpPoint(double.Parse(scoreArgs[2]), double.Parse(scoreArgs[3]));
                } break;
            }
        }

        primaryProcessedScore.Sort((x, y) => (double)x[4] > (double)y[4] ? 1 : (double)x[4] < (double)y[4] ? -1 : 0);

        var processedScore = new List<object[]>();

        foreach (var i in primaryProcessedScore) {
            switch(i[0]) {
                case 1: {
                    CreateNote(
                        Convert.ToChar(i[1]),
                        Convert.ToChar(i[2]),
                        curves[Convert.ToInt32(i[3])],
                        timingPts.GetHitTickByBeat(Convert.ToDouble(i[5])),
                        timingPts.GetPositionTickByBeat(Convert.ToDouble(i[5])) - (long)(Convert.ToDouble(i[4]) * 10000000),
                        timingPts.GetPositionTickByBeat(Convert.ToDouble(i[5])),
                        Convert.ToInt16(i[6]),
                        Convert.ToBoolean(i[7]),
                        Convert.ToBoolean(i[8])
                    );
                } break;
                case 2: {

                } break;
            }
        }
        
        gameStartedTime = DateTime.Now.Ticks + 20000000;
    }

    void Start() {

        LoadScore("bpm_rt");
        musicPlayer.playMusic();

        noteNum = 0;
    }

    void Update() {
        if (Input.GetKey(KeyCode.Space)) gameStartedTime = DateTime.Now.Ticks + 20000000;
        gameMasterTime = DateTime.Now.Ticks - gameStartedTime;
        gameMasterPosition = timingPts.GetPositionTickByTime(gameMasterTime);
    }
}
