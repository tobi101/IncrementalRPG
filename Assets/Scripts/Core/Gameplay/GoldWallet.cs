using System;
using Core.Save;
using IncrementalRPG.Scripts.Core;
using Utils;

namespace Core.Gameplay
{
    public class GoldWallet
    {
        private readonly GoldWalletView _view;
        private readonly SaveService _saveService;

        // public BigDouble Total { get; private set; }
        //
        // public event Action<BigDouble> OnChanged;
        //
        // public GoldWallet(GoldWalletView view, SaveService saveService)
        // {
        //     _view = view;
        //     _saveService = saveService;
        //
        //     Initialize();
        // }
        //
        // public void Initialize()
        // {
        //     _view.Bind(this);
        //     _saveService.LoadFor(this);
        //     OnChanged?.Invoke(Total);
        // }
        //
        // public void Contribute(SaveData data) => data.PlayerInfo.GoldTotal = Total;
        //
        // public void Load(SaveData data)
        // {
        //     Total = data.PlayerInfo.GoldTotal;
        // }
        //
        // public void Update(float deltaTime) { }
        //
        // public void Add(int amount)
        // {
        //     Total += amount;
        //     OnChanged?.Invoke(Total);
        // }
    }
}
