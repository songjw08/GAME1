using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Akila.FPSFramework.Internal;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Pickable")]
    public class Pickable : Item, IInteractable
    {
        [Tooltip("Display name used when showing the interaction prompt.")]
        public string interactionName = "Take";

        [Tooltip("The type of this pickable (Item or Collectable).")]
        public PickableType type;

        [Tooltip("The inventory item prefab to be added when this is picked up (used if type is 'Item').")]
        public InventoryItem item;

        [Tooltip("Unique identifier for this collectable (used if type is 'Collectable').")]
        public InventoryCollectableIdentifier collectableIdentifier;

        [Tooltip("Optional sound clip played when this item is interacted with.")]
        public AudioClip interactSound;

        [Tooltip("The amount of collectables granted when picked up (used if type is 'Collectable').")]
        public int collectableCount = 1;

        [HideInInspector] public UnityEvent<GameObject> onInteractWithItem;
        [HideInInspector] public UnityEvent<GameObject> onInteractWithAmmo;

        public void Interact(InteractionsManager source)
        {
            if (source == null)
            {
                Debug.LogError("InteractionsManager is null during interaction.", gameObject);
                return;
            }

            if (!isActive) return;

            if (interactSound != null)
                source.interactAudio?.PlayOneShot(interactSound);
            else if(source.defaultInteractAudio)
                source.interactAudio?.PlayOneShot(source.defaultInteractAudio.audioClip);

            switch (type)
            {
                case PickableType.Item:
                    InteractWithItem(source);
                    break;
                case PickableType.Collectable:
                    InteractWithCollectable(source);
                    break;
                default:
                    Debug.LogWarning($"Unhandled pickable type: {type}", gameObject);
                    break;
            }
        }

        public string GetInteractionName()
        {
            string info = $"{Name} - {type}";
            return $"{interactionName} {info}";
        }

        public virtual void InteractWithItem(InteractionsManager source)
        {
            if (source == null || source.Inventory == null)
            {
                Debug.LogError("Missing InteractionsManager or Inventory reference during item interaction.", gameObject);
                return;
            }

            GameObject player = source.transform.SearchFor<ICharacterController>()?.gameObject;
            if (player != null)
                onInteractWithItem?.Invoke(player);

            if (!isActive || item == null)
            {
                Debug.LogWarning("Pickable is inactive or item is null during InteractWithItem.", gameObject);
                return;
            }

            IInventory inventory = source.Inventory;

            InventoryItem newItem = Instantiate(item, inventory.transform);
            inventory.items = inventory.transform.GetComponentsInChildren<InventoryItem>(true).ToList();

            int index = inventory.items.IndexOf(newItem);

            if (inventory.items.Count > inventory.maxSlots)
            {
                if (inventory.currentItemIndex >= 0 && inventory.currentItemIndex < inventory.items.Count)
                {
                    inventory.items[inventory.currentItemIndex]?.Drop();
                }

                index = inventory.items.Count - 1;
            }

            index = Mathf.Clamp(index, 0, inventory.maxSlots - 1);
            inventory.Switch(index);

            Destroy(gameObject);
        }

        public virtual void InteractWithCollectable(InteractionsManager source)
        {
            if (source == null || source.Inventory == null)
            {
                Debug.LogError("Missing source or inventory reference during collectable interaction.", gameObject);
                return;
            }

            GameObject player = source.transform.SearchFor<ICharacterController>()?.gameObject;
            if (player != null)
                onInteractWithAmmo?.Invoke(player);

            IInventory inventory = source.Inventory;

            InventoryCollectable collectable = inventory.collectables
                .FirstOrDefault(m => m.identifier == collectableIdentifier);

            if (collectable == null)
            {
                collectable = new InventoryCollectable
                {
                    identifier = collectableIdentifier,
                    count = collectableCount
                };
                inventory.collectables.Add(collectable);
            }
            else
            {
                collectable.count += collectableCount;
            }

            if (inventory.items.Count > 0)
            {
                InventoryItem currentItem = inventory.items[inventory.currentItemIndex];
                if (currentItem?.animators != null)
                {
                    foreach (Animator animator in currentItem.animators)
                    {
                        animator?.CrossFade("Pickup", 0.1f, 0, 0);
                    }
                }
            }

            Destroy(gameObject);
        }

        [ContextMenu("Setup/Network Components")]
        private void SetupNetworkComponents()
        {
#if UNITY_EDITOR
            FPSFrameworkEditor.InvokeConvertMethod("ConvertPickable", this, new object[] { this });
#endif
        }
    }

    public enum PickableType
    {
        Item = 0,
        Collectable = 1,
    }
}
