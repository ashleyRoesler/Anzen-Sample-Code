using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using ReimEnt.Networking;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Anzen/Inventory"), HideMonoScript]
public class InventoryComponent : NetworkMonobehaviour {

    private readonly Logger _logger = new Logger("Inventory");

    public static int ItemCapacity = 48;

    public event Action<InventoryComponent> Changed;
    public event Action ItemObtained;
    public event Action GoldChanged;

    public event Action InventoryFull;

    public int Gold {
        get => _gold;
        set {
            if (value != _gold) {
                _gold = value;
                GoldChanged?.Invoke();
            }
        }
    }

    [SyncedVar(Channel = NetChannels.FullyReliable)]
    private int _gold = 0;

    public List<Item> Items = new List<Item>();

    private void Awake() {
        EnsureCapacity(ItemCapacity);
    }

    private void Start() {
        EnsureCapacity(ItemCapacity);
    }

    public Item this[int index] {
        get => Items[index];
        private set {
            EnsureCapacity(ItemCapacity);

            if (index >= Items.Count)
                throw new ArgumentException($"Cannot insert at index {index}. Max item count is {Items.Count}");

            if (Items[index] != value) {
                Items[index] = value;
                OnSlotChanged(index, value);
            }
        }
    }

    public void RequestSetSlot(int index, Item item) {
        
        if (!IsServer) {
            InvokeServerRpc(Net_RequestSetSlot, index, item, NetChannels.FullyReliable);
            return;
        }

        this[index] = item;
    }

    /// <summary>
    /// Adds an item to the inventory, filling the first empty slot
    /// </summary>
    public int Add(Item item) {

        if (!item) 
            return -1;

        int index = Items.IndexOf(null);

        // only add item if there is space
        if (index == -1) {
            OnInventoryFull();
            return -1;
        }

        RequestSetSlot(index, item);

        OnItemObtained();
        
        return Items.IndexOf(item);
    }

    public void Add(IEnumerable<Item> items) {
        foreach (Item item in items)
            Add(item);
    }

    public int Count => Items.FindAll(i => i != null).Count;

    public bool Contains(Item item) {
        return Items.Contains(item);
    }

    public void Remove(Item item) {
        if (item && Items.Contains(item))
            Remove(Items.IndexOf(item));
    }

    public void RemoveRange(params Item[] items) {
        foreach (Item item in items)
            Remove(item);
    }

    public Item Remove(int slotIndex) {
        Item item = Items[slotIndex];

        Items[slotIndex] = null;
        Changed?.Invoke(this);

        if (IsServer)
            InvokeClientRpcOnEveryoneExcept(Net_Remove, ServerId, slotIndex, NetChannels.FullyReliable);

        return item;
    }

    public void Clear() {
        for (int i = 0; i < Items.Count; i++) {
            Items[i] = null;

            OnSlotChanged(i, null);
        }
    }

    public void Drop(Item item) {
        Drop(Items.IndexOf(item));
    }

    public void Drop(int slotIndex) {

        /// Clients can only request
        if (!IsServer) {
            InvokeServerRpc(Net_RequestDrop, slotIndex, NetChannels.FullyReliable);
            return;
        }

        Item item = Remove(slotIndex);

        var pickablesManager = FindObjectOfType<PickablesManager>();
        if (!pickablesManager)
            throw new Exception("Cannot find PickablesManager");

        if (item)
            pickablesManager.Spawn(item, transform.position);
    }

    public void DropAll() {

        if (!IsAuthoritativeContext)
            throw new Exception("Should only be called in an authoritative context");

        if (IsServer) {

            InvokeClientRpcOnEveryoneExcept(Net_DropAll, ServerId, NetChannels.FullyReliable);

            var pickablesManager = FindObjectOfType<PickablesManager>();
            if (!pickablesManager)
                throw new Exception("Cannot find PickablesManager");

            foreach (Item item in Items.ToArray()) {
                if (item)
                    pickablesManager.Spawn(item, transform.position);
            }
        }

        RemoveRange(Items.ToArray());
    }

    protected void EnsureCapacity(int capacity) {
        if (Items.Count < capacity) {
            Items.SetLength(capacity, () => null);
        }
    }

    protected void Trim() {
        for (int i = Items.Count - 1; i >= 0; i--) {
            if (Items[i] == null)
                Items.RemoveAt(i);
             else 
                break;
        }
    }

    protected void OnSlotChanged(int slot, Item item) {
        Changed?.Invoke(this);

        _logger.Log($"({gameObject}) Setting slot {slot} to {item}");

        if (IsServer)
            InvokeClientRpcOnEveryoneExcept(Net_SetSlot, ServerId, slot, item, NetChannels.FullyReliable);
    }

    protected override void OnPlayerJoined(ulong clientId) {
        InvokeClientRpcOnClient(Net_Clear, clientId, NetChannels.ReliableSequenced);

        for (int i = 0; i < Items.Count; i++) {
            Item item = Items[i];
            if (item != null)
                InvokeClientRpcOnClient(Net_SetSlot, clientId, i, item, NetChannels.ReliableSequenced);
        }
    }

    private void OnInventoryFull() {
        InventoryFull?.Invoke();

        if (IsServer)
            InvokeClientRpcOnEveryoneExcept(Net_InventoryFull, ServerId, NetChannels.FullyReliable);
    }

    private void OnItemObtained() {
        ItemObtained?.Invoke();

        if (IsServer)
            InvokeClientRpcOnEveryoneExcept(Net_ItemObtained, ServerId, NetChannels.FullyReliable);
    }

    [ServerRPC]
    protected void Net_RequestSetSlot(int index, Item item) => this[index] = item;

    [ServerRPC]
    protected void Net_RequestDrop(int slotIndex) => Drop(slotIndex);

    [ClientRPC]
    protected void Net_SetSlot(int index, Item item) => this[index] = item;

    [ClientRPC]
    protected void Net_Remove(int index) => Remove(index);

    [ClientRPC]
    protected void Net_Clear() => Clear();

    [ClientRPC]
    protected void Net_DropAll() => DropAll();

    [ClientRPC]
    protected void Net_InventoryFull() => OnInventoryFull();

    [ClientRPC]
    protected void Net_ItemObtained() => OnItemObtained();
}