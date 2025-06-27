using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using TMPro;

namespace Akila.FPSFramework
{
    public static class ExtensionMethods
    {
        #region Component
        /// <summary>
        /// Tries to find T on game object then child then parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T SearchFor<T>(this Component component, bool includeInactive = false)
        {
            if (component.GetComponent<T>() != null) return component.GetComponent<T>();
            if (component.GetComponentInChildren<T>(includeInactive) != null) return component.GetComponentInChildren<T>(includeInactive);

            return component.GetComponentInParent<T>(includeInactive);
        }

        /// <summary>
        /// Tries to find Component on game object then child then parent
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Component SearchFor(this Component component, Type type)
        {
            if (component.GetComponent(type) != null) return component.GetComponent(type);
            if (component.GetComponentInChildren(type) != null) return component.GetComponentInChildren(type);

            return component.GetComponentInParent(type);
        }
        #endregion

        #region Transform
        /// <summary>
        /// Sets transform position to given position
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position">Target position</param>
        /// <param name="local">If true position is going to chnage in local space insted of global space</param>
        public static void SetPosition(this Transform transform, Vector3 position, bool local = false)
        {
            if (local) transform.localPosition = position;
            else transform.position = position;
        }

        /// <summary>
        /// Sets transform rotation to given rotation
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rotation">Target rotation</param>
        /// <param name="local">If true position is going to chnage in local space insted of global space</param>
        public static void SetRotation(this Transform transform, Quaternion rotation, bool local = false)
        {
            if (local) transform.localRotation = rotation;
            else transform.rotation = rotation;
        }

        /// <summary>
        /// Resets transform position, rotation & scale
        /// </summary>
        /// <param name="transform"></param>
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Transform CreateChild(this Transform transform)
        {
            Transform children = new GameObject("GameObject").transform;

            children.parent = transform;
            children.Reset();

            return children;
        }

        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="name">Name of the child</param>
        /// <returns></returns>
        public static Transform CreateChild(this Transform transform, string name)
        {
            Transform children = new GameObject(name).transform;

            children.parent = transform;
            children.Reset();

            return children;
        }


        /// <summary>
        /// Adds a new game object as a child of the transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="names">Target names the more names you have the more childern you get</param>
        /// <param name="parentAll">If true all childern will be child if each other</param>
        /// <returns></returns>
        public static Transform[] CreateChildren(this Transform transform, string[] names, bool parentAll = false)
        {
            List<Transform> transforms = new List<Transform>();

            for (int i = 0; i < names.Length; i++)
            {
                Transform child = CreateChild(transform, names[i]);
                transforms.Add(child);

                if (parentAll)
                {
                    if (i > 1)
                    {
                        transforms[1].SetParent(transforms[0]);

                        child.SetParent(transforms[transforms.Count - 2]);
                    }
                }
            }

            return transforms.ToArray();
        }

        /// <summary>
        /// destroies all childern in transform
        /// </summary>
        /// <param name="transform"></param>
        public static void ClearChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void SetPositionAndRotation(this Transform transform, Vector3 position, Quaternion rotation, bool local = false)
        {
            if (!local)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }

        public static void SetPositionAndRotation(this Transform transform, Vector3 position, Vector3 eulerAngles, bool local = false)
        {
            if (!local)
            {
                transform.position = position;
                transform.eulerAngles = eulerAngles;
            }
            else
            {
                transform.localPosition = position;
                transform.localEulerAngles = eulerAngles;
            }
        }

        public static Vector3 GetDirection(this Transform transform, Vector3Direction direction)
        {
            Vector3 dir = Vector3.zero;

            switch (direction)
            {
                case Vector3Direction.forward:
                    dir = transform.forward;
                    break;

                case Vector3Direction.back:
                    dir = -transform.forward;

                    break;

                case Vector3Direction.right:
                    dir = transform.right;

                    break;

                case Vector3Direction.left:
                    dir = -transform.right;

                    break;

                case Vector3Direction.up:
                    dir = transform.up;

                    break;

                case Vector3Direction.down:
                    dir = -transform.up;

                    break;
            }

            return dir;
        }
        #endregion

        #region Character Controller
        public static bool IsVelocityZero(this CharacterController characterController)
        {
            //check if player is standing still if yes set to true else set to false
            return characterController.velocity.magnitude <= 0;
        }
        #endregion

        #region Rigidbody
        public static bool IsVelocityZero(this Rigidbody rigidbody)
        {
            //check if rigidbody is not moving if yes set to true else set to false
            return rigidbody.velocity.magnitude <= 0;
        }
        #endregion

        #region Dropdown
        public static void AddOption(this Dropdown dropdown, string option)
        {
            dropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData() { text = option } });
        }
        #endregion

        #region Input Action
        /// <summary>
        /// Checks for douple clicks and sets targetValue to true if the user has douple clicked
        /// </summary>
        /// <param name="inputAction"></param>
        /// <param name="targetValue"></param>
        /// <param name="lastClickTime"></param>
        /// <returns></returns>
        public static void HasDoupleClicked(this InputAction inputAction, ref bool targetValue, ref float lastClickTime, float maxClickTime = 0.5f)
        {
            if (inputAction.triggered)
            {
                float timeSinceLastSprintClick = Time.time - lastClickTime;

                if (timeSinceLastSprintClick < maxClickTime)
                {
                    targetValue = true;
                }

                lastClickTime = Time.time;
            }

            if (inputAction.IsPressed() == false) targetValue = false;
        }
        #endregion

        #region Resolution
        public static string GetDetails(this Resolution resolution)
        {
            return $"{resolution.height}x{resolution.width} {resolution.refreshRateRatio.value}Hz";
        }
        #endregion

        #region Animator
        /// <summary>
        /// Checks if the specified animation state is currently playing in the animator.
        /// </summary>
        /// <param name="animationStateName">The name of the animation state to check if it's playing.</param>
        /// <returns>True if the specified animation state is currently playing; otherwise, false.</returns>
        public static bool IsPlaying(this Animator animator, string animationStateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor Only - Adds a parameter to the animator's controller with the specified name and type.
        /// If the parameter already exists and overwrite is false, a message is logged and the parameter is not added.
        /// </summary>
        /// <param name="name">The name of the parameter to add.</param>
        /// <param name="type">The type of the parameter (e.g., Float, Int, Bool).</param>
        /// <param name="overwrite">Whether to overwrite the parameter if it already exists.</param>
        public static void AddParameter(this Animator animator, string name, AnimatorControllerParameterType type, bool overwrite)
        {
            // Check if the animator controller already has a parameter with the given name
            if (!overwrite && HasParameter(animator, name))
            {
                // If overwrite is false and the parameter already exists, log a message and exit
                Debug.Log($"Animator on {animator.gameObject.name} already has Parameter with the name ({name}).");
                return;
            }

            // Get the AnimatorController from the runtimeAnimatorController
            UnityEditor.Animations.AnimatorController animatorController = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;

            // Create and configure a new AnimatorControllerParameter
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                type = type,
                name = name
            };

            // Add the new parameter to the animator controller
            animatorController.AddParameter(parameter);
        }


        /// <summary>
        /// Editor Only - Checks if the specified parameter exists in the animator's parameter list.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to check for.</param>
        /// <returns>True if the parameter exists; otherwise, false.</returns>
        public static bool HasParameter(this Animator animator, string parameterName)
        {
            // Loop through each parameter in the animator's parameters list
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                // Check if the parameter's name matches the given name
                if (param.name == parameterName)
                    return true; // Parameter found
            }
            return false; // Parameter not found
        }
#endif

        #endregion

        #region List
        public static int GetLength<T>(this List<T> list)
        {
            int length = list.Count - 1;
            
            length = Mathf.Clamp(length, 0, list.Count - 1);

            return length;
        }
        public static void MoveElement<T>(this List<T> list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= list.Count ||
                newIndex < 0 || newIndex >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Indices are out of range.");
            }

            T element = list[oldIndex];
            list.RemoveAt(oldIndex);

            // Adjust targetIndex if necessary after removal
            if (newIndex > oldIndex)
            {
                newIndex--;
            }

            list.Insert(newIndex, element);
        }

        public static void MoveElementUp<T>(this List<T> list, int oldIndex)
        {
            if (oldIndex > 0 && oldIndex < list.Count)
            {
                (list[oldIndex - 1], list[oldIndex]) = (list[oldIndex], list[oldIndex - 1]);
            }
        }

        public static void MoveElementDown<T>(this List<T> list, int oldIndex)
        {
            if (oldIndex >= 0 && oldIndex < list.Count - 1)
            {
                (list[oldIndex], list[oldIndex + 1]) = (list[oldIndex + 1], list[oldIndex]);
            }
        }

        #endregion

        #region Button

#if UNITY_EDITOR
        [MenuItem("CONTEXT/Button/Upgrade")]
        public static void Upgrade()
        {
            GameObject obj = Selection.activeGameObject;

            Button button = obj.GetComponent<Button>();

            InteractiveButton interactiveButton = (InteractiveButton)Undo.AddComponent(obj, typeof(InteractiveButton));


            interactiveButton.interactable = button.interactable;
            interactiveButton.targetGraphics = button.targetGraphic;
            interactiveButton.targetText = obj.GetComponentInChildren<TextMeshProUGUI>();

            interactiveButton.normalGraphicsColor = button.colors.normalColor;
            interactiveButton.highlightedGraphicsColor = button.colors.highlightedColor;
            interactiveButton.selectedGraphicsColor = button.colors.selectedColor;
            interactiveButton.disabledGraphicsColor = button.colors.disabledColor;

            interactiveButton.colorMultiplier = button.colors.colorMultiplier;
            interactiveButton.fadeDuration = button.colors.fadeDuration;

            interactiveButton.onClick = button.onClick;


            Undo.DestroyObjectImmediate(button);
        }

#endif
#endregion
    }
}