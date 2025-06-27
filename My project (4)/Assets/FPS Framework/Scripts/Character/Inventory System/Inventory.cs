using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Inventory")]
    public class Inventory : MonoBehaviour, IInventory
    {
        public List<InventoryItem> startItems = new List<InventoryItem>();
        public List<InventoryItem> items = new List<InventoryItem>();
        public List<InventoryCollectable> collectables = new List<InventoryCollectable>();

        [Space]
        public InventoryItem defaultItem;
        public int maxSlots = 3;
        public float dropForce = 1;
        public Transform dropLocation;

        public CharacterManager characterManager { get; set; }

        public CharacterInput characterInput { get; set; }

        public bool isActive { get; set; } = true;


        private int previousItemIndex { get; set; }
        public int currentItemIndex { get; set; }


        List<InventoryItem> IInventory.items { get => items; set => items = value; }
        int IInventory.maxSlots { get => maxSlots; }
        float IInventory.dropForce {  get => dropForce; }
        Transform IInventory.dropPoint { get => dropLocation; }
        public bool isInputActive { get; set; } = true;

        public InventoryItem _defaultItem { get; set; }
        List<InventoryItem> IInventory.startItems { get => startItems; set => startItems = value; }
        InventoryItem IInventory.defaultItem { get => defaultItem; set => defaultItem = value; }
        public InventoryItem currentDefaultItem { get => _defaultItem; set => _defaultItem = value; }
        List<InventoryCollectable> IInventory.collectables { get => collectables; }

        private void Start()
        {
            if (items.Count > 0)
            {
                Debug.LogWarning($"[Warning] Items list has {items.Count} elements, but it should be empty. This may indicate an unexpected state. Emptying the list now.", this);

                items.Clear();
            }

            characterManager = GetComponentInParent<CharacterManager>();

            characterInput = GetComponentInParent<CharacterInput>();

            if (isActive)
            {
                foreach (InventoryItem startItem in startItems)
                {
                    InventoryItem newItem = Instantiate(startItem, transform);
                }

                foreach (InventoryItem item in GetComponentsInChildren<InventoryItem>())
                {
                    InventoryItem newItem = Instantiate(item, transform);

                    Destroy(item.gameObject);
                }

                if (defaultItem)
                {
                    _defaultItem = Instantiate(defaultItem, transform);
                    _defaultItem.isDroppingActive = false;
                }
            }

            Switch(0);
        }

        private void Update()
        {
            GetInput();

            //Ensure the item index wraps around correctly, staying within the bounds of the list
            if (currentItemIndex > items.ToArray().Length - 1) currentItemIndex = 0;

            if (!_defaultItem)
            {
                if (currentItemIndex < 0) currentItemIndex = items.ToArray().Length - 1;
            }
            else
            {
                if (currentItemIndex < -1) currentItemIndex = items.ToArray().Length - 1;
            }

            //Switch item if item index changes
            if (previousItemIndex != currentItemIndex)
            {
                //Empty
                lastItemIndex = previousItemIndex;
            }
            
            Switch(currentItemIndex, false);

            
            //Update the item index
            previousItemIndex = currentItemIndex;
        }

        private void GetInput()
        {
            if (!isInputActive) return;

            if (characterInput.controls.Player.Item1.triggered) currentItemIndex = 0;
            if (characterInput.controls.Player.Item2.triggered && items.Count >= 2) currentItemIndex = 1;
            if (characterInput.controls.Player.Item3.triggered && items.Count >= 3) currentItemIndex = 2;
            if (characterInput.controls.Player.Item4.triggered && items.Count >= 4) currentItemIndex = 3;
            if (characterInput.controls.Player.Item5.triggered && items.Count >= 5) currentItemIndex = 4;
            if (characterInput.controls.Player.Item6.triggered && items.Count >= 6) currentItemIndex = 5;
            if (characterInput.controls.Player.Item7.triggered && items.Count >= 7) currentItemIndex = 6;
            if (characterInput.controls.Player.Item8.triggered && items.Count >= 8) currentItemIndex = 7;
            if (characterInput.controls.Player.Item9.triggered && items.Count >= 9) currentItemIndex = 8;
            
            if (characterInput.controls.Player.SwitchItem.ReadValue<float>() > 0) currentItemIndex++;

            if (characterInput.controls.Player.SwitchItem.ReadValue<float>() < 0)
            {
                currentItemIndex--;
                
                {
                    if (currentItemIndex < 0) currentItemIndex = items.ToArray().Length - 1;
                }
            }

            if (characterInput.controls.Player.DefaultItem.triggered) currentItemIndex = -1;

            if(characterInput.controls.Player.NextItem.triggered) currentItemIndex++;
            if (characterInput.controls.Player.PreviousItem.triggered) currentItemIndex = lastItemIndex;
        }

        private int lastItemIndex;

        public void Switch(int index, bool immediate = true)
        {
            List<InventoryItem> childrenItems = GetComponentsInChildren<InventoryItem>(true).ToList();

            if(_defaultItem)
            {
                childrenItems.Remove(_defaultItem);
            }

            if (childrenItems.Count == 0)
            {
                    _defaultItem?.gameObject.SetActive(true);
            }

            items = childrenItems;

            currentItemIndex = index;

            items = childrenItems;

            if (index == -1)
            {
                _defaultItem?.gameObject.SetActive(true);

                foreach(InventoryItem item in childrenItems) 
                {
                    item.gameObject.SetActive(false);
                }

                return;
            }

            int num = 0;

            if(_defaultItem) _defaultItem.gameObject.SetActive(false);

            foreach (InventoryItem obj in items)
            {
                if (num == index)
                {
                    obj.gameObject.SetActive(true);
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
                num++;
            }

        }

        public void DropAllItems()
        {
            foreach (InventoryItem item in items)
            {
                if (item != null)
                {
                    item.Drop(false);
                }
            }

            items.Clear();
        }

#if UNITY_EDITOR
        [ContextMenu("Setup/Network Components")]
        public void Convert()
        {
            bool notFound = true;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == "FPSFrameworkComponentsManager")
                    {
                        type.GetMethod("ConvertInventory").Invoke(this, new object[] { this });
                        notFound = false;
                    }
                }
            }

            if (notFound)
            {
                Debug.LogError("Please install 'FPS Framework: Multiplayer Edition' before trying to network your components.");
            }
        }
#endif
    }
}