using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static EditorSequencer;

public class EditorSequencerPianoRoll : EditorWindow
{
    public const int TopOffset = 18;
    public const int KeyHeight = 25;
    public const int KeyWidth = 100;
    public const int MaxKeys = 128;
    public const int GridInterval = 100;
    public const int GridIntervalSubdivisions = 4;
    public const float Lerpness = 0.8f;

    private static bool MouseDown { get; set; } = false;

    private Material glMaterial;
    private Vector2 scroll = new Vector2(0f, MaxKeys / 2f);
    private Vector2 lerpedScroll = new Vector2(0f, MaxKeys / 2f);
    private int instrument;
    private float zoom = 1f;
    private float lerpedZoom = 1f;
    private Vector2? dragOffset;
    private Note noteDragging;
    private Vector2 lastMousePosition;
    private bool showInstrumentsPopup;

    [MenuItem("Synthesizer/Sequencer/Piano roll")]
    public static void Initialize()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow<EditorSequencerPianoRoll>("Piano roll");
    }

    public static void Initialize(InstancedPattern instancedPattern, Track track)
    {
        EditorSequencerPianoRoll window = GetWindow<EditorSequencerPianoRoll>("Piano roll");

        var pattern = track.uniquePatterns[instancedPattern.pattern];
        Current = pattern;
        window.instrument = 0;
    }

    public static void Initialize(Pattern pattern)
    {
        EditorSequencerPianoRoll window = GetWindow<EditorSequencerPianoRoll>("Piano roll");
        
        Current = pattern;
        window.instrument = 0;
    }

    public static Pattern Current { get; private set; }

    private void OnEnable()
    {
        scroll.y = MaxKeys / 2f;
        lerpedScroll.y = scroll.y;

        zoom = 10f;
        lerpedZoom = zoom;

        var shader = Shader.Find("Hidden/Internal-Colored");
        glMaterial = new Material(shader);

        dragOffset = null;
        noteDragging = null;
    }

    private void OnDisable()
    {
        DestroyImmediate(glMaterial);
    }

    private void Update()
    {
        lerpedScroll.x = Mathf.Lerp(lerpedScroll.x, scroll.x, Time.deltaTime * Lerpness);
        lerpedScroll.y = Mathf.Lerp(lerpedScroll.y, scroll.y, Time.deltaTime * Lerpness);
    }

    private void OnGUI()
    {
        Mouse();

        Grid();
        Guides();
        Notes();
        Keys();
        Toolbar();
        Bars();
        Instruments();

        if (Event.current.isScrollWheel)
        {
            scroll.y += Event.current.delta.y;
            if (Event.current.keyCode == KeyCode.LeftControl)
            {
                Debug.Log("zoom");
            }
        }

        if(Event.current.button == 2)
        {
            scroll -= new Vector2(Event.current.delta.x, Event.current.delta.y / -KeyHeight);
            lerpedScroll.y = scroll.y;
        }

        Repaint();
    }

    private void Bars()
    {
        EditorGUI.DrawRect(new Rect(Screen.width - 15, TopOffset, 15, 15), new Color(0.15f, 0.15f, 0.15f, 1f));

        if(showInstrumentsPopup)
        {
            GUI.enabled = false;
            GUI.color = Color.white;
            GUI.VerticalScrollbar(new Rect(Screen.width - 15, TopOffset + 15, 18, Screen.height - 18 - 18), scroll.y, 1f / MaxKeys, MaxKeys, 0f);
            GUI.HorizontalScrollbar(new Rect(0, TopOffset, Screen.width - 15, 18f), scroll.x, 50f, 0f, 2000f);
        }
        else
        {
            GUI.enabled = true;
            scroll.y = GUI.VerticalScrollbar(new Rect(Screen.width - 15, TopOffset + 15, 18, Screen.height - 18 - 18), scroll.y, 1f / MaxKeys, MaxKeys, 0f);
            scroll.x = GUI.HorizontalScrollbar(new Rect(0, TopOffset, Screen.width - 15, 18f), scroll.x, 50f, 0f, 2000f);
        }
    }

    private void Toolbar()
    {
        //draw toolbar background
        GUI.Box(new Rect(0, 0, Screen.width, TopOffset), "", EditorStyles.toolbarButton);

        //display the toolbar top
        Rect rect = new Rect(0, 0, 200, TopOffset);
        if (!showInstrumentsPopup)
        {
            string name = instrument >= 0 && EditorSequencer.Current.instruments.Count > instrument ? EditorSequencer.Current.instruments[instrument] : "None";
            if (GUI.Button(rect, "Instrument: " + name, EditorStyles.toolbarButton))
            {
                if (EditorSequencer.Current.instruments.Count > 0)
                {
                    showInstrumentsPopup = true;
                }
                else
                {
                    throw new System.Exception("There are no instruments in this track, add one via Synthesizer/Sequencer/Instruments/Add Instrument");
                }
            }
        }

        if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            if (!rect.Contains(Event.current.mousePosition))
            {
                showInstrumentsPopup = false;
            }
        }
    }

    private void Instruments()
    {
        if (showInstrumentsPopup)
        {
            GUI.enabled = true;
            Rect rect = new Rect(0, 0, 200, TopOffset);
            //Debug.Log(Event.current.button == 0 && Event.current.type == EventType.MouseDown);
            int index = 0;
            foreach (var instrument in EditorSequencer.Current.instruments)
            {
                rect.y += TopOffset;
                if (DrawBox(rect, instrument) || (rect.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.type == EventType.MouseDown))
                {
                    this.instrument = index;
                    showInstrumentsPopup = false;
                    break;
                }
                index++;
            }
        }
    }

    private void Guides()
    {
        //draw black keys later
        for (int i = 0; i <= MaxKeys; i++)
        {
            bool isBlack = IsBlack(i);
            if (!isBlack) continue;

            float y = Screen.height - (KeyHeight * i) + (lerpedScroll.y * KeyHeight) + 8;
            if (y + KeyHeight > 0 && y < Screen.height)
            {
                y = y % Screen.height;

                //draw the line guides
                EditorGUI.DrawRect(new Rect(KeyWidth, TopOffset + y + 6, Screen.width - KeyWidth, KeyHeight), Color.black * 0.3f);
            }
        }
    }

    private void Notes()
    {
        Vector2 velocity = lastMousePosition - Event.current.mousePosition;
        //Debug.Log(velocity);

        //draw all of the notes from the pattern
        if (Current != null)
        {
            bool selected = false;
            //draw each note
            foreach (var note in Current.notes[instrument].notes)
            {
                int s_start = Mathf.RoundToInt(note.start * lerpedZoom);
                int s_end = Mathf.RoundToInt(note.end * lerpedZoom);

                float start = KeyWidth + s_start - scroll.x;
                float length = s_end - s_start;
                float y = Screen.height - (KeyHeight * note.note) + (lerpedScroll.y * KeyHeight) + 8;

                Rect rect = new Rect(start, TopOffset + y + 6, length, KeyHeight);
                bool contains = rect.Contains(Event.current.mousePosition);
                if (contains && dragOffset == null && noteDragging == null && MouseDown)
                {
                    //drag note
                    noteDragging = note;
                    selected = true;
                    dragOffset = new Vector2(s_start + KeyWidth, y) - Event.current.mousePosition;
                    //Debug.Log("Started dragging note " + dragOffset);
                }
                else if (MouseDown && dragOffset != null && noteDragging != null)
                {
                    if (note == noteDragging)
                    {
                        selected = true;

                        int noteLength = s_end - s_start;
                        float x = (Event.current.mousePosition.x - KeyWidth + dragOffset.Value.x);

                        noteDragging.note = (byte)(Mathf.RoundToInt((Screen.height - Event.current.mousePosition.y + TopOffset + (lerpedScroll.y * KeyHeight)) / KeyHeight) + 1);

                        s_start = Mathf.CeilToInt(Mathf.RoundToInt(x / GridIntervalSubdivisions) * GridIntervalSubdivisions);
                        s_end = s_start + noteLength;

                        note.start = Mathf.RoundToInt(s_start / lerpedZoom);
                        note.end = Mathf.RoundToInt(s_end / lerpedZoom);
                        //Debug.Log("Dragging note");
                    }
                }
                else if (!MouseDown)
                {
                    if (dragOffset != null && noteDragging != null)
                    {
                        //Debug.Log("Finished dragging note");
                        dragOffset = null;

                        noteDragging = null;
                        selected = true;
                        break;
                    }
                }

                //right clicked, so delete it
                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    if(rect.Contains(Event.current.mousePosition))
                    {
                        Current.notes[instrument].notes.Remove(note);
                        break;
                    }
                }

                EditorGUI.DrawRect(rect, Color.white);
            }

            //clicked down, on nothing, place note at mouse position
            if (!selected && Event.current.type == EventType.MouseUp)
            {
                //left clicked
                if (Event.current.button == 0)
                {
                    //Debug.Log("Added note");

                    float x = (Event.current.mousePosition.x - KeyWidth + lerpedScroll.x);
                    byte note = (byte)(Mathf.RoundToInt((Screen.height - Event.current.mousePosition.y + TopOffset + (lerpedScroll.y * KeyHeight)) / KeyHeight) + 1);
                    float s_start = Mathf.CeilToInt(Mathf.RoundToInt(x / GridIntervalSubdivisions) * GridIntervalSubdivisions);
                    float s_end = s_start + 50;

                    Note newNote = new Note(note, Mathf.RoundToInt(s_start / lerpedZoom), Mathf.RoundToInt(s_end / lerpedZoom));
                    Current.notes[instrument].notes.Add(newNote);
                }
            }
        }

        lastMousePosition = Event.current.mousePosition;
    }

    private void Grid()
    {
        Rect rect = GUILayoutUtility.GetRect(0, 0, Screen.width, Screen.height);
        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(rect);
            //GL.PushMatrix();
            GL.Clear(true, false, Color.black * 0.1f);
            glMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(Color.black * 0.1f);

            //horizontal lines for each key
            for (int i = 0; i < MaxKeys; i++)
            {
                float y = Screen.height - (KeyHeight * i) + (lerpedScroll.y * KeyHeight) + 8;
                if (y > 0 && y < Screen.height)
                {
                    GL.Vertex3(0, y, 0);
                    GL.Vertex3(Screen.width, y, 0);
                }
            }

            //vertical snapping lines
            float x = KeyWidth;
            while (true)
            {
                if (x >= KeyWidth)
                {
                    GL.Color(Color.black * 0.2f);
                    GL.Vertex3(x - scroll.x, 0, 0);
                    GL.Vertex3(x - scroll.x, Screen.height, 0);

                    for (int i = 0; i < GridIntervalSubdivisions; i++)
                    {
                        float remap = Remap(i, 0, GridIntervalSubdivisions, 0f, GridInterval);
                        GL.Color(Color.black * 0.07f);
                        GL.Vertex3(x - scroll.x + remap, 0, 0);
                        GL.Vertex3(x - scroll.x + remap, Screen.height, 0);
                    }
                }

                x += GridInterval;
                if (x - scroll.x > Screen.width + GridInterval) break;
            }

            GL.End();
            GUI.EndClip();
        }
    }

    private bool IsBlack(int note)
    {
        int normalized = note - (Mathf.FloorToInt(note / 12f) * 12);
        bool isBlack = normalized == 1;   //A#
        isBlack |= normalized == 3;       //C#
        isBlack |= normalized == 5;       //D#
        isBlack |= normalized == 8;       //F#
        isBlack |= normalized == 10;      //G#

        return isBlack;
    }

    private void Keys()
    {
        //draw key shadow
        EditorGUI.DrawRect(new Rect(KeyWidth, TopOffset, 8, Screen.height), Color.black * 0.3f);

        //draw white keys first
        for (int i = 0; i <= MaxKeys; i++)
        {
            bool isBlack = IsBlack(i);
            if (isBlack) continue;

            float y = Screen.height - (KeyHeight * i) + (lerpedScroll.y * KeyHeight) + 8;
            if (y + KeyHeight > 0 && y < Screen.height)
            {
                y = y % Screen.height;

                Rect rect = new Rect(0, TopOffset + y, KeyWidth, KeyHeight);

                //draw key shadow if its black on top of the white key below
                Rect whiteKey = new Rect(rect);

                //if theres a key above, extend the upper bounds
                bool topBlack = IsBlack(i + 1);
                bool bottomBlack = IsBlack(i - 1);
                if (topBlack)
                {
                    whiteKey.yMin -= KeyHeight / 2f;
                }
                if (bottomBlack)
                {
                    whiteKey.yMax += KeyHeight / 2f;
                }

                EditorGUI.DrawRect(whiteKey, Color.gray);

                int octave = Mathf.FloorToInt(i / 12f);
                string letter = Helper.GetLetterFromNote(i - 7);
                GUI.Label(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, 30), letter + " " + octave);
            }
        }

        //draw black keys later
        for (int i = 0; i <= MaxKeys; i++)
        {
            bool isBlack = IsBlack(i);
            if (!isBlack) continue;

            float y = Screen.height - (KeyHeight * i) + (lerpedScroll.y * KeyHeight) + 8;
            if (y + KeyHeight > 0 && y < Screen.height)
            {
                y = y % Screen.height;

                Rect rect = new Rect(0, TopOffset + y, KeyWidth - 10, KeyHeight);

                //draw key shadow if its black on top of the white key below
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + KeyHeight, rect.width, 5), Color.black * 0.3f);
                EditorGUI.DrawRect(rect, Color.black);

                int octave = Mathf.FloorToInt(i / 12f);
                string letter = Helper.GetLetterFromNote(i - 7);
                GUI.Label(new Rect(rect.x + 5, rect.y + 5, rect.width - 10 + 10, 30), letter + " " + octave);
            }
        }
    }

    private void Mouse()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            MouseDown = true;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            MouseDown = false;
        }
    }
}