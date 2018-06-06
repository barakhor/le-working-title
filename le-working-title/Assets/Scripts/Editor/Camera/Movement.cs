using UnityEditor;
using UnityEngine;

namespace Editor.Camera
{
    [CustomEditor(typeof(global::Camera.Movement))]
    public class CameraMovementEditor : UnityEditor.Editor
    {
        private global::Camera.Movement Camera
        {
            get {return target as global::Camera.Movement;}
        }

        private TabsBlock tabs;

        private void OnEnable()
        {
            tabs = new TabsBlock(new System.Collections.Generic.Dictionary<string, System.Action>()
            {
                { "Movement", MovementTab },
                { "Rotation", RotationTab },
                { "Height", HeightTab }
            });
            tabs.SetCurrentMethod(Camera.LastTab);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Camera, "CameraMovement");
            tabs.Draw();
            if(GUI.changed)
                Camera.LastTab = tabs.CurrentMethodIndex;
            EditorUtility.SetDirty(Camera);
        }

        private void MovementTab()
        {
            using(new HorizontalBlock())
            {
                GUILayout.Label("Use keyboard input: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseKeyboardInput = EditorGUILayout.Toggle(Camera.UseKeyboardInput);
            }

            if(Camera.UseKeyboardInput)
            {
                Camera.HorizontalAxis =
                    EditorGUILayout.TextField("Horizontal axis name: ", Camera.HorizontalAxis);
                Camera.VerticalAxis =
                    EditorGUILayout.TextField("Vertical axis name: ", Camera.VerticalAxis);
                Camera.KeyboardMovementSpeed =
                    EditorGUILayout.FloatField("Movement speed: ", Camera.KeyboardMovementSpeed);
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Screen edge input: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseScreenEdgeInput = EditorGUILayout.Toggle(Camera.UseScreenEdgeInput);
            }

            if(Camera.UseScreenEdgeInput)
            {
                EditorGUILayout.FloatField("Screen edge border size: ", Camera.ScreenEdgeBorder);
                Camera.ScreenEdgeMovementSpeed =
                    EditorGUILayout.FloatField("Screen edge movement speed: ",
                                               Camera.ScreenEdgeMovementSpeed);
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Panning with mouse: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UsePanning = EditorGUILayout.Toggle(Camera.UsePanning);
            }

            if(Camera.UsePanning)
            {
                Camera.PanningKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Panning when holding: ",
                                                        Camera.PanningKey);
                Camera.PanningSpeed = EditorGUILayout.FloatField("Panning speed: ", Camera.PanningSpeed);
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Limit movement: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.LimitMap = EditorGUILayout.Toggle(Camera.LimitMap);
            }

            if(Camera.LimitMap)
            {
                Camera.LimitX = EditorGUILayout.FloatField("Limit X: ", Camera.LimitX);
                Camera.LimitY = EditorGUILayout.FloatField("Limit Y: ", Camera.LimitY);
            }

            GUILayout.Label("Follow target", EditorStyles.boldLabel);
            Camera.TargetFollow =
                EditorGUILayout.ObjectField("Target to follow: ", Camera.TargetFollow,
                                            typeof(Transform), true) as Transform;
            Camera.TargetOffset   = EditorGUILayout.Vector3Field("Target offset: ", Camera.TargetOffset);
            Camera.FollowingSpeed = EditorGUILayout.FloatField("Following speed: ", Camera.FollowingSpeed);
        }

        private void RotationTab()
        {
            using(new HorizontalBlock())
            {
                GUILayout.Label("Keyboard input: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseKeyboardRotation = EditorGUILayout.Toggle(Camera.UseKeyboardRotation);
            }

            if(Camera.UseKeyboardRotation)
            {
                Camera.RotateLeftKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Rotate left: ", Camera.RotateLeftKey);
                Camera.RotateRightKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Rotate right: ",
                                                        Camera.RotateRightKey);
                Camera.RotationSped =
                    EditorGUILayout.FloatField("Keyboard rotation speed", Camera.RotationSped);
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Mouse input: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseMouseRotation = EditorGUILayout.Toggle(Camera.UseMouseRotation);
            }

            if(!Camera.UseMouseRotation)
            {
                return;
            }

            Camera.MouseRotationKey =
                (KeyCode) EditorGUILayout.EnumPopup("Mouse rotation key: ",
                                                    Camera.MouseRotationKey);
            Camera.MouseRotationSpeed =
                EditorGUILayout.FloatField("Mouse rotation speed: ", Camera.MouseRotationSpeed);
        }

        private void HeightTab()
        {
            using(new HorizontalBlock())
            {
                GUILayout.Label("Auto height: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.AutoHeight = EditorGUILayout.Toggle(Camera.AutoHeight);
            }

            if(Camera.AutoHeight)
            {
                Camera.HeightDampening =
                    EditorGUILayout.FloatField("Height dampening: ", Camera.HeightDampening);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundMask"));
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Keyboard zooming: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseKeyboardZooming = EditorGUILayout.Toggle(Camera.UseKeyboardZooming);
            }

            if(Camera.UseKeyboardZooming)
            {
                Camera.ZoomInKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Zoom In: ", Camera.ZoomInKey);
                Camera.ZoomOutKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Zoom Out: ", Camera.ZoomOutKey);
                Camera.KeyboardZoomingSensitivity =
                    EditorGUILayout.FloatField("Keyboard sensitivity: ", Camera.KeyboardZoomingSensitivity);
            }

            using(new HorizontalBlock())
            {
                GUILayout.Label("Scrollwheel zooming: ", EditorStyles.boldLabel,
                                GUILayout.Width(170f));
                Camera.UseScrollwheelZooming = EditorGUILayout.Toggle(Camera.UseScrollwheelZooming);
            }

            if(Camera.UseScrollwheelZooming)
                Camera.ScrollWheelZoomingSensitivity =
                    EditorGUILayout.FloatField("Scrollwheel sensitivity: ",
                                               Camera.ScrollWheelZoomingSensitivity);

            if(!Camera.UseScrollwheelZooming && !Camera.UseKeyboardZooming)
            {
                return;
            }

            using(new HorizontalBlock())
            {
                Camera.MinHeight = EditorGUILayout.FloatField("Min height: ", Camera.MinHeight);
                Camera.MaxHeight = EditorGUILayout.FloatField("Max height: ", Camera.MaxHeight);
            }
        }
    }
}
