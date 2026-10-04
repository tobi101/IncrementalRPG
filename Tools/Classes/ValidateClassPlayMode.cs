using System;
using System.Linq;
using System.Reflection;
using Core.Classes;
using Core.Gameplay;
using Core.Gameplay.Dungeon;
using Core.Save;
using Core.StateMachine;
using Core.StateMachine.Features;
using Core.StateMachine.States;
using Core.Items;
using Model;
using UI;
using UI.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Object=UnityEngine.Object;

public static class ValidateClassPlayMode
{
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static int checks;
    static void Check(bool ok,string name){checks++;if(!ok)throw new Exception("FAILED: "+name);}
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Flags).GetValue(obj);
    static object Call(object obj,string method,params object[] args)=>obj.GetType().GetMethod(method,Flags).Invoke(obj,args);
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,Flags).SetValue(obj,value);
    static ClassesMenuView View()=>Object.FindFirstObjectByType<ClassesMenuView>(FindObjectsInactive.Include);
    static void Guard()=>Check(UnityEditor.EditorApplication.isPlaying && new SaveStorage().SavePath.StartsWith("/private/tmp/IncrementalRPG-classes"),"isolated Play Mode save");
    public static string Onboarding()
    {
        checks=0;Guard();var view=View();var service=Field<ClassProgressionService>(view,"_service");var catalog=Field<ClassCatalog>(view,"_catalog");
        var machine=Field<GameStateMachine>(view,"_machine");var gameplay=Field<GameplayFeature>(machine,"_gameplayFeature");
        var hub=Object.FindFirstObjectByType<HubView>(FindObjectsInactive.Include);var shrine=Field<HubFeatureButtonView>(hub,"_shrineButton");
        Check(!service.HasChosenFirstClass&&!service.FirstChoicePending&&shrine.Button.interactable,"Shrine available before first choice");
        machine.Enter<HubState>();
        shrine.Button.onClick.Invoke();
        Check(machine.IsCurrent<ClassesMenuState>()&&view.mainRoot.activeInHierarchy&&!view.choiceRoot.activeSelf,"Shrine previews classes before introduction");
        Check(view.lockedRoot.activeInHierarchy&&!view.unlockButton.gameObject.activeSelf,"premature class purchase hidden");
        Call(view,"ReturnToHub");
        Check(machine.IsCurrent<HubState>()&&!view.navigationRoot.gameObject.activeInHierarchy,"preview returns to hub and hides class navigation");
        machine.Enter<GameplayState>();gameplay.StartSession();
        for(int i=0;i<catalog.introductionDungeon.LevelCount;i++)
        {
            if(i>0){Call(gameplay,"ApplyLevel",i);Set(gameplay,"_pendingLevelTransitionIndex",-1);}
            Check(!service.FirstChoicePending,"entering level "+i+" does not unlock classes");
            var progress=Field<object>(gameplay,"_levelExperience");progress.GetType().GetMethod("Add",BindingFlags.Instance|BindingFlags.Public).Invoke(progress,new object[]{gameplay.CurrentLevelExperienceGoal});
            Call(gameplay,"HandleEnemyKilled",new SpawnService.EntityDestroyedContext(null,null,Vector2Int.zero,Vector3.zero));
            if(i<catalog.introductionDungeon.LevelCount-1)
            {
                Check(Field<int>(gameplay,"_pendingLevelTransitionIndex")==i+1,"XP completion queues next level");
                Check(!service.FirstChoicePending,"intermediate XP completion does not unlock classes");
            }
        }
        Check(Field<bool>(gameplay,"_pendingDemoLimit"),"last level queues terminal flow");
        Call(gameplay,"BeginLootGrace",-1,true);Call(gameplay,"TickLootGrace",1000f);
        Check(service.FirstChoicePending,"last level XP completion records introduction");
        Check(machine.IsCurrent<GameplayState>(),"results shown before choosing");
        var popup=Object.FindFirstObjectByType<SessionEndPopupView>(FindObjectsInactive.Include);
        Check(popup.gameObject.activeInHierarchy,"results popup visible");
        Field<Button>(popup,"_hubButton").onClick.Invoke();
        Check(machine.IsCurrent<ClassesMenuState>()&&view.choiceRoot.activeInHierarchy,"closing results opens class choice");
        Check(!view.mainRoot.activeInHierarchy,"first choice exclusive");
        Check(new SaveStorage().Read().DungeonProgressState.IsCompleted(catalog.introductionDungeon.dungeonId),"pending choice persisted");
        return "PASS: "+checks+" Play Mode onboarding assertions. Pending choice intentionally left saved for restart test.";
    }
    public static string RestartAndChoose()
    {
        checks=0;Guard();var view=View();var service=Field<ClassProgressionService>(view,"_service");var machine=Field<GameStateMachine>(view,"_machine");
        Check(service.FirstChoicePending&&machine.IsCurrent<ClassesMenuState>()&&view.choiceRoot.activeInHierarchy,"pending first choice restored automatically on startup");
        Call(view,"ReturnToHub");Check(machine.IsCurrent<ClassesMenuState>(),"mandatory choice cannot be skipped to hub");
        view.choices[0].button.onClick.Invoke();Check(service.HasChosenFirstClass,"choice card selects class");
        return "PASS: "+checks+" restart assertions.";
    }
    public static string MenuInteractions()
    {
        checks=0;Guard();var v=View();var service=Field<ClassProgressionService>(v,"_service");var catalog=Field<ClassCatalog>(v,"_catalog");
        var machine=Field<GameStateMachine>(v,"_machine");var player=Field<Player>(v,"_player");
        machine.Enter<ClassesMenuState>();player.GoldTotal=100000;
        Check(!v.resetButton.interactable,"last class reset button disabled");
        var fire=catalog.classes[0];var ice=catalog.classes[1];
        foreach(var n in fire.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack||n.reward==ClassNodeReward.RuneKey))
            service.Purchase(fire,n);
        v.classTabs[1].button.onClick.Invoke();
        Check(v.lockedRoot.activeInHierarchy&&v.unlockButton.interactable,"locked class view displays affordable unlock");
        v.unlockButton.onClick.Invoke();Call(v,"Refresh");
        Check(service.IsUnlocked(ice)&&!v.lockedRoot.activeSelf&&v.resetButton.interactable,"unlock button spends key and opens board");
        v.attacksTab.onClick.Invoke();
        var attack=fire.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Special).attack;
        var item=v.GetComponentsInChildren<ClassAttackItemView>().First(i=>i.Attack==attack&&!i.IsEquipped);
        var ev=new PointerEventData(EventSystem.current){pointerDrag=item.gameObject,button=PointerEventData.InputButton.Left};
        v.attackColumns[0].equipped.GetComponent<ClassAttackDropZone>().OnDrop(ev);
        Check(service.Equipped(AttackSlot.Special)==null,"wrong UI drop target rejected");
        v.attackColumns[2].equipped.GetComponent<ClassAttackDropZone>().OnDrop(ev);Call(v,"Refresh");
        Check(service.Equipped(AttackSlot.Special)==attack,"drag drop equips correct type");
        var equipped=v.GetComponentsInChildren<ClassAttackItemView>().First(i=>i.Attack==attack&&i.IsEquipped);
        ev.pointerDrag=equipped.gameObject;
        v.attackColumns[2].content.GetComponentInParent<ScrollRect>().GetComponent<ClassAttackDropZone>().OnDrop(ev);Call(v,"Refresh");
        Check(service.Equipped(AttackSlot.Special)==null,"drop back to list unequips");
        var manual=fire.nodes.First(n=>n.reward==ClassNodeReward.UnlockAttack&&n.attack.slot==AttackSlot.Manual).attack;
        v.SetAttack(AttackSlot.Manual,manual);Call(v,"Refresh");
        item=v.GetComponentsInChildren<ClassAttackItemView>().First(i=>i.Attack==manual&&i.IsEquipped);
        item.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
        Check(service.Equipped(AttackSlot.Manual)==null,"equipped attack click clears slot");
        v.classesTab.onClick.Invoke();v.classTabs[0].button.onClick.Invoke();v.resetButton.onClick.Invoke();
        Check(v.confirmation.activeSelf,"reset confirmation visible");
        v.cancelButton.onClick.Invoke();Check(service.IsUnlocked(fire)&&!v.confirmation.activeSelf,"cancel keeps progression");
        v.resetButton.onClick.Invoke();v.confirmButton.onClick.Invoke();Call(v,"Refresh");
        Check(!service.IsUnlocked(fire)&&service.IsUnlocked(ice)&&v.lockedRoot.activeSelf,"confirmed reset closes only selected class");
        v.classTabs[1].button.onClick.Invoke();Check(!v.resetButton.interactable,"remaining last class protected in UI");
        Call(v,"ReturnToHub");
        var hub=Object.FindFirstObjectByType<HubView>(FindObjectsInactive.Include);var shrine=Field<HubFeatureButtonView>(hub,"_shrineButton");
        Check(machine.IsCurrent<HubState>()&&shrine.Button.interactable,"return to hub restores usable Shrine");
        return "PASS: "+checks+" Play Mode menu assertions (buttons, drop handlers, reset confirmation and return to hub). Run ShrineNavigation on a later frame, after HubView.Start has registered its listeners.";
    }
    public static string ShrineNavigation()
    {
        checks=0;Guard();var v=View();var machine=Field<GameStateMachine>(v,"_machine");
        Check(machine.IsCurrent<HubState>(),"hub active before Shrine click");
        var hub=Object.FindFirstObjectByType<HubView>(FindObjectsInactive.Include);
        Field<HubFeatureButtonView>(hub,"_shrineButton").Button.onClick.Invoke();
        Check(machine.IsCurrent<ClassesMenuState>()&&v.mainRoot.activeInHierarchy,"Shrine reopens menu");
        return "PASS: "+checks+" Shrine navigation assertions.";
    }
}
