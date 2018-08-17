using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static EditorSequencer;

public class EditorSequencerPlaylist : EditorWindow
{
    public const int TrackHeight = 100;
    public const int TopOffset = 18;
    public const int LeftOffset = 140;
    public const int SmallestUnit = 20;

    private static bool MouseDown { get; set; } = false;

    private Material glMaterial;
    private Vector2? dragOffset;
    private InstancedPattern patternDragging;
    private Vector2 dragStart;
    private float dragDistance;

    [MenuItem("Synthesizer/Sequencer/Playlist")]
    public static void Initialize()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow<EditorSequencerPlaylist>("Playlist");
    }

    private void OnEnable()
    {
        var shader = Shader.Find("Hidden/Internal-Colored");
        glMaterial = new Material(shader);
    }

    private void OnDisable()
    {
        DestroyImmediate(glMaterial);
    }

    private void OnGUI()
    {
        Repaint();

        Toolbar();
        Drag();
        Grid();
        Mouse();

        //draw the tracks
        int tracks = Mathf.CeilToInt(Screen.width / MaxTracks);
        for (int i = 0; i < tracks; i++)
        {
            Rect position = new Rect(0, TopOffset + i * TrackHeight, LeftOffset, TrackHeight);
            if (i < MaxTracks)
            {
                GUI.Box(position, "Track " + (i + 1), EditorStyles.helpBox);
            }
            else
            {
                GUI.Box(position, "", EditorStyles.helpBox);
            }
        }
        
        //draw the patterns
        foreach (var pattern in Current.patterns)
        {
            Rect rect = new Rect(LeftOffset + pattern.time, TopOffset + pattern.order * TrackHeight, pattern.GetLength(Current), TrackHeight);
            
            if (MouseDown && rect.Contains(Event.current.mousePosition) && dragOffset == null)
            {
                //start dragging the pattern
                dragOffset = GetPatternPosition(Event.current.mousePosition - pattern.Position - Vector2.down * TopOffset);
                patternDragging = pattern;
                dragStart = Event.current.mousePosition;
                dragDistance = 0f;
                //Debug.Log("Started dragging " + pattern.Name);
            }
            else if (MouseDown && dragOffset != null)
            {
                if(pattern == patternDragging)
                {
                    //snap the position of the pattern to the mouse
                    pattern.time = GetPatternPosition().x - LeftOffset - (int)dragOffset.Value.x;
                    pattern.order = (byte)(((uint)GetPatternPosition().y - (uint)dragOffset.Value.y) / (float)TrackHeight);
                    dragDistance = Vector2.Distance(dragStart, Event.current.mousePosition);
                }
            }
            else if (!MouseDown && dragOffset != null)
            {
                //stop dragging
                //Debug.Log("Stopped dragging");
                patternDragging = null;
                dragOffset = null;
            }

            //display the pattern box
            if (DrawBox(rect, pattern.name))
            {
                //Debug.Log(dragDistance);
                if(dragDistance < 1f)
                {
                    EditorSequencerPianoRoll.Initialize(pattern, Current);
                }
            }

            //display the pattern notes
            DrawNotes(rect, pattern);
        }
    }

    private void DrawNotes(Rect rect, InstancedPattern pattern)
    {
        var original = Current.uniquePatterns[pattern.pattern];
        byte highest = (byte)(original.Highest + 2);
        byte lowest = (byte)(original.Lowest - 2);

        //Debug.Log(highest + ", " + lowest);

        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(rect);
            //GL.PushMatrix();
            GL.Clear(true, false, Color.black * 1f);
            glMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(Color.white * 1f);

            //draw note lines
            for (int i = 0; i < Current.instruments.Count; i++)
            {
                foreach (var note in original.notes[i].notes)
                {
                    float remap = Remap(note.note, lowest, highest, rect.height, 0);
                    GL.Vertex3(note.start, remap, 0);
                    GL.Vertex3(note.end, remap, 0);
                }
            }
            
            GL.End();
            GUI.EndClip();
        }
    }

    private void Toolbar()
    {
        //draw toolbar background
        GUI.Box(new Rect(0, 0, Screen.width, TopOffset), "", EditorStyles.toolbarButton);

        //display the toolbar top
        Rect rect = new Rect(0, 0, 100, TopOffset);
        if (GUI.Button(rect, "New", EditorStyles.toolbarButton))
        {
            New();
        }

        rect.x += 100;
        if (GUI.Button(rect, "Load", EditorStyles.toolbarButton))
        {
            string path = EditorUtility.OpenFilePanel("Open sequencer data...", Application.dataPath, Extension);
            Load(path);
        }

        rect.x += 100;
        if (GUI.Button(rect, "Save", EditorStyles.toolbarButton))
        {
            Save();
        }
    }

    private void Drag()
    {
        //visualize where the pattern would drop to
        if (DragAndDrop.visualMode == DragAndDropVisualMode.Link)
        {
            int? index = (int)DragAndDrop.GetGenericData("pattern_index");
            var mouse = GetPatternPosition();
            Rect position = new Rect(mouse.x, mouse.y, Current.uniquePatterns[index.Value].Duration, TrackHeight);
            EditorGUI.DrawRect(position, new Color(1f, 1f, 1f, 0.2f));
        }

        if (Event.current.type == EventType.DragUpdated)
        {
            // called every frame that a drag operation is occuring and the mouse is being dragged
            int? index = (int)DragAndDrop.GetGenericData("pattern_index");
            if (index != null)
            {
                var mouse = Event.current.mousePosition;
                if (mouse.x > LeftOffset && mouse.y < TrackHeight * MaxTracks)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
            }
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            // called on the frame that the dragged object gets dropped (if it's still inside this window)
            int? index = (int)DragAndDrop.GetGenericData("pattern_index");
            if(index != null)
            {
                var mouse = GetPatternPosition();
                DragAndDrop.AcceptDrag();

                Pattern pattern = Current.uniquePatterns[index.Value];
                InstancedPattern instancedPattern = new InstancedPattern(pattern.name, index.Value)
                {
                    time = mouse.x - LeftOffset,
                    order = (byte)(mouse.y / (float)TrackHeight)
                };
                Current.patterns.Add(instancedPattern);
            }
        }
    }

    private Vector2Int GetPatternPosition(Vector2? mouse = null)
    {
        if (mouse == null) mouse = Event.current.mousePosition;

        var vector = new Vector2(mouse.Value.x, mouse.Value.y - TopOffset);
        vector.x = Mathf.Round((vector.x - SmallestUnit / 2f) / SmallestUnit) * SmallestUnit;
        vector.y = Mathf.Round((vector.y - TrackHeight / 2f) / TrackHeight) * TrackHeight;
        vector.y += TopOffset;

        return new Vector2Int((int)vector.x, (int)vector.y);
    }

    private void Grid()
    {
        Rect rect = GUILayoutUtility.GetRect(10, 1000, 200, 200);
        if (Event.current.type == EventType.Repaint)
        {
            GUI.BeginClip(rect);
            //GL.PushMatrix();
            GL.Clear(true, false, Color.black * 0.1f);
            glMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(Color.black * 0.1f);

            //horizontal lines
            for (int i = 0; i < MaxTracks; i++)
            {
                GL.Vertex3(LeftOffset, TopOffset + i * TrackHeight, 0);
                GL.Vertex3(Screen.width, TopOffset + i * TrackHeight, 0);
            }

            //vertical lines
            float lines = (Screen.width - LeftOffset) / (float)SmallestUnit;
            for (int i = 0; i < lines; i++)
            {
                //draw a bigger divisor
                if (i % 4 == 0)
                {
                    GL.Color(Color.black * 0.3f);
                    GL.Vertex3(LeftOffset + i * SmallestUnit, TopOffset, 0);
                    GL.Vertex3(LeftOffset + i * SmallestUnit, Screen.height, 0);
                }
                //draw a normal divider
                else
                {
                    GL.Color(Color.black * 0.07f);
                    GL.Vertex3(LeftOffset + i * SmallestUnit, TopOffset, 0);
                    GL.Vertex3(LeftOffset + i * SmallestUnit, Screen.height, 0);
                }
            }
            
            GL.End();
            GUI.EndClip();
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