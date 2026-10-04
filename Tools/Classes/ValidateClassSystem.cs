using System;
using System.Linq;
using System.Collections.Generic;
using Core.Classes;
using Core.Gameplay.Dungeon;
using Core.Items;
using Core.Save;
using Model;
using UnityEditor;
using UnityEngine;
using Utils;

public static class ValidateClassSystem
{
    static int checks;
    static void Assert(bool condition,string name){checks++;if(!condition)throw new Exception("FAILED: "+name);}
    public static string Run()
    {
        checks=0;
        var catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>("Assets/SO/Classes/ClassCatalog.asset");
        var items=AssetDatabase.FindAssets("t:ItemCatalog").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ItemCatalog>).First();
        Assert(catalog.classes.Length==3,"three classes");
        Assert(items.Get(catalog.runeKey.itemId)==catalog.runeKey,"key in inventory catalog");
        var ids=new HashSet<string>();
        foreach(var c in catalog.classes)
        {
            Assert(c.emblem!=null && c.banner!=null,"class art");
            foreach(var n in c.nodes)
            {
                Assert(ids.Add(n.id),"unique node ID "+n.id);
                Assert(n.icon!=null,"node icon "+n.id);
                for(int l=0;l<n.LevelLimit;l++) Assert(n.TryGetCost(l,out _),"explicit cost "+n.id);
                Assert(n.requirements.All(r=>c.nodes.Contains(r.node)&&r.level<=r.node.LevelLimit),"local prerequisites "+n.id);
                var path=new HashSet<ClassNodeDefinition>();
                Action<ClassNodeDefinition> visit=null;visit=node=>{Assert(path.Add(node),"acyclic graph "+node.id);foreach(var r in node.requirements)visit(r.node);path.Remove(node);};visit(n);
            }
            var unlocked=c.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack).Select(n=>n.attack).ToArray();
            Assert(unlocked.Count(a=>a.slot==AttackSlot.Manual)==1,"manual type");
            Assert(unlocked.Count(a=>a.slot==AttackSlot.Automatic)==1,"auto type");
            Assert(unlocked.Count(a=>a.slot==AttackSlot.Special)==3,"special types");
        }
        var data=new SaveData();var player=new Player();player.Load(data);player.GoldTotal=100000;
        var storage=new PlayerItemStorage(items);storage.Load(data);
        var dungeons=new DungeonSelectionService();dungeons.Load(data);
        var service=new ClassProgressionService(catalog,dungeons,player,storage);service.Load(data);
        var fire=catalog.classes[0];var ice=catalog.classes[1];var earth=catalog.classes[2];
        Assert(!service.FirstChoicePending&&!service.ChooseFirst(fire),"fresh save gated");
        dungeons.MarkLevelReached(catalog.introductionDungeon,catalog.introductionDungeon.LevelCount-1);
        Assert(!service.FirstChoicePending,"reaching final level is insufficient");
        dungeons.MarkCompleted(catalog.introductionDungeon);
        Assert(service.FirstChoicePending,"completion unlocks choice");
        Assert(service.ChooseFirst(fire)&&!service.ChooseFirst(ice),"first choice exactly once");
        Assert(service.Equipped(AttackSlot.Manual)==catalog.defaultManualAttack&&service.Equipped(AttackSlot.Automatic)==null&&service.Equipped(AttackSlot.Special)==null,"initial loadout");
        Assert(!service.CanReset(fire)&&!service.Reset(fire),"last class reset denied");
        var autoFire=fire.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Automatic);
        var strikeFire=fire.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Manual);
        var before=player.GoldTotal;
        Assert(!service.Purchase(fire,autoFire)&&player.GoldTotal==before,"prerequisite failure does not charge");
        player.GoldTotal=0;Assert(!service.Purchase(fire,strikeFire),"insufficient gold denied");player.GoldTotal=before;
        Assert(service.Purchase(fire,strikeFire),"purchase unlocks manual attack");
        var upgrade=fire.nodes.First(n=>n.reward==ClassNodeReward.UpgradeAttack&&n.requirements.All(r=>r.node==strikeFire));
        var upgradeStart=player.GoldTotal;var upgradeSpent=BigDouble.Zero;
        for(int i=0;i<upgrade.LevelLimit;i++)
        {
            upgrade.TryGetCost(i,out var cost);upgradeSpent+=cost;
            Assert(service.Purchase(fire,upgrade)&&service.Level(fire,upgrade)==i+1,"multi-level purchase "+i);
        }
        Assert(player.GoldTotal==BigDoubleMath.RoundToInteger(upgradeStart-upgradeSpent),"each upgrade charges its configured level price");
        before=player.GoldTotal;
        Assert(!service.Purchase(fire,upgrade)&&player.GoldTotal==before,"maximum level rejects another charge");
        Assert(service.Equip(AttackSlot.Manual,strikeFire.attack),"equip unlocked attack");
        Assert(!service.Equip(AttackSlot.Special,strikeFire.attack),"wrong slot rejected");
        var keyFire=fire.nodes.First(n=>n.reward==ClassNodeReward.RuneKey);
        foreach(var n in fire.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack)) if(service.Level(fire,n)==0) Assert(service.Purchase(fire,n),"unlock fire branch prerequisite");
        Assert(service.Purchase(fire,keyFire)&&service.RuneKeys==1,"key reward enters inventory");
        Assert(!service.Purchase(fire,keyFire)&&service.RuneKeys==1,"key cannot be farmed twice within cycle");
        Assert(service.Unlock(ice)&&service.RuneKeys==0,"unlock atomically spends key");
        Assert(!service.Unlock(ice)&&!service.Unlock(earth),"duplicate and unaffordable unlock denied");
        var strikeIce=ice.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Manual);
        var autoIce=ice.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Automatic);
        Assert(service.Purchase(ice,strikeIce)&&service.Purchase(ice,autoIce)&&service.Equip(AttackSlot.Automatic,autoIce.attack),"mixed class loadout");
        before=player.GoldTotal;var refund=service.Refund(fire);
        Assert(refund==BigDoubleMath.RoundToInteger(upgradeSpent+2000),"refund ledger includes every upgrade price");
        Assert(service.Reset(fire)&&!service.IsUnlocked(fire)&&player.GoldTotal==BigDoubleMath.RoundToInteger(before+refund),"reset closes class and refunds actual expenditure");
        Assert(service.Equipped(AttackSlot.Manual)==null&&service.Equipped(AttackSlot.Automatic)==autoIce.attack,"reset only clears owner slots");
        Assert(!service.Reset(ice),"remaining last class protected");
        var keyIce=ice.nodes.First(n=>n.reward==ClassNodeReward.RuneKey);
        foreach(var n in ice.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack)) if(service.Level(ice,n)==0) Assert(service.Purchase(ice,n),"unlock ice branch prerequisite");
        Assert(service.Purchase(ice,keyIce)&&service.Unlock(fire),"reopen reset class using key");
        Assert(service.Level(fire,strikeFire)==0&&service.Refund(fire)==0,"reopened class starts empty");
        foreach(var n in fire.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack)) Assert(service.Purchase(fire,n),"rebuild fire branch");
        Assert(service.Purchase(fire,keyFire)&&service.RuneKeys==1,"key reward repeatable after reset");
        Assert(service.Unlock(earth),"third class unlocks");
        var strikeEarth=earth.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Manual);
        var specialEarth=earth.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Special);
        Assert(service.Purchase(earth,strikeEarth)&&service.Purchase(earth,specialEarth)&&service.Equip(AttackSlot.Special,specialEarth.attack),"special earth attack assigned");
        Assert(service.Equip(AttackSlot.Manual,strikeFire.attack),"three classes can mix");
        Assert(service.Equip(AttackSlot.Manual,null),"empty manual slot permitted after unequip");
        var saved=new SaveData();player.Contribute(saved);storage.Contribute(saved);dungeons.Contribute(saved);service.Contribute(saved);
        var roundtrip=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(saved));
        var restoredPlayer=new Player();restoredPlayer.Load(roundtrip);var restoredItems=new PlayerItemStorage(items);restoredItems.Load(roundtrip);
        var restoredDungeons=new DungeonSelectionService();restoredDungeons.Load(roundtrip);
        var restored=new ClassProgressionService(catalog,restoredDungeons,restoredPlayer,restoredItems);restored.Load(roundtrip);
        Assert(restored.UnlockedCount==3&&!restored.FirstChoicePending,"class ownership survives save");
        Assert(restored.Equipped(AttackSlot.Manual)==null&&restored.Equipped(AttackSlot.Automatic)==autoIce.attack&&restored.Equipped(AttackSlot.Special)==specialEarth.attack,"mixed and empty slots survive save");
        Assert(restored.Refund(earth)==service.Refund(earth),"refund ledger survives save");
        var pendingData=new SaveData();dungeons.Contribute(pendingData);var pending=new ClassProgressionService(catalog,dungeons,player,storage);pending.Load(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(pendingData)));
        Assert(pending.FirstChoicePending,"unfinished first choice restored after restart");
        var legacy=JsonUtility.FromJson<SaveData>("{\"Version\":3}");var oldDungeons=new DungeonSelectionService();oldDungeons.Load(legacy);var oldService=new ClassProgressionService(catalog,oldDungeons,player,storage);oldService.Load(legacy);
        Assert(!oldService.HasChosenFirstClass&&!oldService.FirstChoicePending,"legacy save accepted without granting class");
        return "PASS: "+checks+" assertions (catalog, prerequisites, transactions, key cycles, resets, mixed loadout and JSON migration). No player save read or written.";
    }
}
