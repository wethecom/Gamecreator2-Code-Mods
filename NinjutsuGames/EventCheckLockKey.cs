using System;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 0, 1)]
    [Title("Check Lock and Key")]
    [Category("Inventory/Lock And Key")]
    [Description("Executed while a game object stays inside the Trigger collider and checks for an item")]

    [Image(typeof(IconTriggerStay), ColorTheme.Type.Blue)]
    
    [Keywords("Item", "Check", "Inventory", "Bag", "Trigger", "Lock", "Key")]

    [Serializable]
    public class EventCheckLockKey : TEventPhysics  
    {
        // The key to press for checking
        [SerializeField] 
        private KeyCode m_KeyCode = KeyCode.E;

        // Reference to the bag to check
        [SerializeField] 
        private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();

        // Reference to the bag to check
        //[SerializeField]
       // private HashSet<KeyCode> currentlyPressedKeys = new HashSet<KeyCode>();
        // The item to look for
        [SerializeField] 
        private PropertyGetItem m_Key = new PropertyGetItem();

        private bool isInTrigger = false;
        private GameObject currentTarget = null;

        protected  override void OnTriggerStay3D(Trigger trigger, Collider collider)
        {
            base.OnTriggerStay3D(trigger, collider);
            
            if (!this.IsActive) return;
            if (!this.isInTrigger || this.currentTarget == null) return;
            if (!Input.GetKeyDown(this.m_KeyCode)) return;

            Args args = new Args(trigger.gameObject, this.currentTarget);
            
            // Get the bag reference
            var bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return;

            // Get the item to check for
            var item = this.m_Key.Get(args);
            if (item == null) return;

            // Check if the bag has the item
            bool hasItem = bag.Content.CountType(item) > 0;
            
            if (hasItem)
            {
                _ = this.m_Trigger.Execute(this.currentTarget);
            }
        }

        protected  override void OnTriggerEnter3D(Trigger trigger, Collider collider)
        {
            if (!this.Match(collider.gameObject)) return;
            
            this.isInTrigger = true;
            this.currentTarget = collider.gameObject;
        }

        protected  override void OnTriggerExit3D(Trigger trigger, Collider collider)
        {
            if (!this.Match(collider.gameObject)) return;
            
            this.isInTrigger = false;
            this.currentTarget = null;
        }


    }
}