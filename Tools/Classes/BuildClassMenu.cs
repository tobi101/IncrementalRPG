using System;
using System.IO;
using System.Linq;
using Core.Classes;
using TMPro;
using UI;
using UI.Classes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class BuildClassMenu
{
    const string Art="Assets/Art/UI/NEW/";
    const string Prefabs="Assets/Prefabs/Classes";
    static TMP_FontAsset header, body;
    static Material textMaterial;
    static Sprite frame, title, tile, decor;
    static Color ink=new Color(.9f,.87f,.8f);
    static ClassesMenuView view;
    public static string Run()
    {
        if(EditorApplication.isPlaying) throw new Exception("Stop Play Mode before building the class menu.");
        Directory.CreateDirectory(Prefabs);
        var catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>("Assets/SO/Classes/ClassCatalog.asset");
        header=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Dynamo CG/dynamocg SDF.asset");
        body=header;
        var textMaterialPath=Prefabs+"/ClassText.mat";
        textMaterial=AssetDatabase.LoadAssetAtPath<Material>(textMaterialPath);
        if(textMaterial==null)
        {
            textMaterial=new Material(header.material){name="ClassText"};
            AssetDatabase.CreateAsset(textMaterial,textMaterialPath);
        }
        textMaterial.SetFloat("_OutlineWidth",.08f);
        textMaterial.SetColor("_OutlineColor",new Color32(12,10,10,255));
        EditorUtility.SetDirty(textMaterial);
        frame=S("Choose class/card_classes.png"); title=S("ассеты ковка/new_title.png");
        tile=S("NEW icon/base_1.png"); decor=S("Class level up/title_decor.png");
        var old=Object.FindFirstObjectByType<ClassesMenuView>(FindObjectsInactive.Include);
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
        var pauseFields=new SerializedObject(pause);
        var sideMenus=pauseFields.FindProperty("_sideMenus");
        var sideMenuIndex=-1;
        if(old!=null)
            for(int i=0;i<sideMenus.arraySize;i++)
                if(sideMenus.GetArrayElementAtIndex(i).objectReferenceValue==old.sideMenu)sideMenuIndex=i;
        if(old!=null) Object.DestroyImmediate(old.gameObject);
        var parent=Object.FindFirstObjectByType<MenuCanvasView>(FindObjectsInactive.Include).transform;
        var viewRoot=Rect("ClassesView",parent,Vector2.zero,new Vector2(1920,1080)); Stretch(viewRoot);
        var composition=Rect("Composition",viewRoot,Vector2.zero,new Vector2(1920,1080));
        viewRoot.gameObject.AddComponent<ClassMenuLayout>().composition=composition;
        view=viewRoot.gameObject.AddComponent<ClassesMenuView>();
        var blocker=Image("InputSurface",composition,null,Vector2.zero,new Vector2(1920,1080));blocker.color=Color.clear;blocker.raycastTarget=true;
        Image("ChainLeft",viewRoot,S("ассеты ковка/Chain_1.png"),new Vector2(-720,-131),new Vector2(142,440));
        Image("ChainLeftSmall",viewRoot,S("ассеты ковка/Chain_2.png"),new Vector2(-535,-85),new Vector2(100,280));
        Image("ChainRight",viewRoot,S("ассеты ковка/Chain_1.png"),new Vector2(735,-131),new Vector2(140,440));
        Image("ChainRightSmall",viewRoot,S("ассеты ковка/Chain_2.png"),new Vector2(540,-85),new Vector2(100,280));
        foreach(RectTransform chain in viewRoot)
            if(chain.name.StartsWith("Chain"))chain.anchorMin=chain.anchorMax=new Vector2(.5f,1);
        composition.SetAsLastSibling();
        var menuPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SideMenu.prefab");
        view.sideMenu=((GameObject)PrefabUtility.InstantiatePrefab(menuPrefab,viewRoot)).GetComponent<SideMenuFlyoutView>();
        if(sideMenuIndex<0){sideMenuIndex=sideMenus.arraySize;sideMenus.arraySize++;}
        sideMenus.GetArrayElementAtIndex(sideMenuIndex).objectReferenceValue=view.sideMenu;pauseFields.ApplyModifiedPropertiesWithoutUndo();
        var choices=Rect("FirstChoice",composition,Vector2.zero,new Vector2(1920,1080));view.choiceRoot=choices.gameObject;
        Title(choices,"classes.choose",new Vector2(0,432),new Vector2(790,140),44);
        Image("ChoiceSigil",choices,S("Choose class/penta_decor.png"),new Vector2(0,375),new Vector2(750,750));
        choices.GetChild(choices.childCount-1).SetAsFirstSibling();
        view.choices=new ClassChoiceCard[3];
        for(int i=0;i<3;i++)
        {
            var def=catalog.classes[i];
            var card=Rect("Choose_"+def.id,choices,new Vector2((i-1)*456,-150),new Vector2(372,660));
            var back=Card(card,def.pattern,new Vector2(372,660),out _);
            var glow=Image("Glow",card,def.sigil,new Vector2(0,295),new Vector2(370,370));
            var group=glow.gameObject.AddComponent<CanvasGroup>();group.alpha=0;
            var emblem=Medallion(card,def,new Vector2(0,295),310);
            var plate=Image("NamePlate",card,S("Choose class/title_"+new[]{"pyro","kryo","geo"}[i]+".png"),new Vector2(0,99),new Vector2(300,52));
            var name=Text("Name",plate.transform,Vector2.zero,new Vector2(268,44),31,true);
            Image("Divider",card,decor,new Vector2(0,24),new Vector2(246,23));
            var description=Text("Description",card,new Vector2(0,-113),new Vector2(282,225),27,false);
            Label("ChooseHint",card,"classes.choose_hint",new Vector2(0,-272),new Vector2(292,44),21);
            var clickable=card.gameObject.AddComponent<Image>();clickable.color=Color.clear;
            var choose=card.gameObject.AddComponent<Button>();choose.targetGraphic=back;
            var hover=card.gameObject.AddComponent<ClassHoverMotion>();hover.emblem=(RectTransform)emblem.transform;hover.glow=group;
            view.choices[i]=new ClassChoiceCard{button=choose,title=name,description=description};
        }
        var main=Rect("ClassManagement",composition,Vector2.zero,new Vector2(1920,1080));view.mainRoot=main.gameObject;
        view.classesTab=Button(main,"ClassesTab","classes.classes",new Vector2(-215,468),new Vector2(375,98),36);
        view.attacksTab=Button(main,"AttacksTab","classes.attacks",new Vector2(215,468),new Vector2(375,98),36);
        Image("HeaderDivider",main,decor,new Vector2(0,412),new Vector2(280,18));
        var classes=Rect("Classes",main,Vector2.zero,new Vector2(1920,1080));view.classesRoot=classes.gameObject;
        view.classTabs=new ClassTabButton[3];
        for(int i=0;i<3;i++)
        {
            var back=Image("Tab_"+catalog.classes[i].id,classes,catalog.classes[i].banner,new Vector2((i-1)*357,340),new Vector2(346,146));
            back.raycastTarget=true;
            var tab=back.gameObject.AddComponent<Button>();tab.targetGraphic=back;
            var text=Text("Name",back.transform,new Vector2(51,0),new Vector2(204,45),27,true);
            view.classTabs[i]=new ClassTabButton{button=tab,background=back,title=text};
        }
        var content=Rect("ClassContent",classes,new Vector2(0,-82),new Vector2(1920,770));
        view.boardSigil=Image("BoardSigil",content,catalog.classes[0].sigil,new Vector2(0,7),new Vector2(935,935));
        view.boardSigil.color=new Color(1,1,1,.26f);
        view.nodesRoot=Rect("Nodes",content,Vector2.zero,new Vector2(910,765));
        view.nodePrefab=CreateNodePrefab();
        var left=Rect("SkillDetails",content,new Vector2(-700,-11),new Vector2(374,487));
        Card(left,catalog.classes[0].pattern,new Vector2(374,487),out view.detailPattern);
        view.detailIcon=Medallion(left,catalog.classes[0],new Vector2(0,241),178);
        var namePlate=Image("NamePlate",left,S("Choose class/title_pyro.png"),new Vector2(0,119),new Vector2(314,59));
        view.detailTitle=Text("Name",namePlate.transform,Vector2.zero,new Vector2(275,49),25,true);view.detailTitle.fontSizeMin=17;
        Image("Divider",left,decor,new Vector2(0,68),new Vector2(258,20));
        Image("PreviewFrame",left,S("NEW popup/frane_opisanie_1.png"),new Vector2(0,-2),new Vector2(287,102));
        view.detailPreview=Image("Preview",left,null,new Vector2(0,-2),new Vector2(85,85));view.detailPreview.preserveAspect=true;
        view.detailDescription=Text("Description",left,new Vector2(0,-103),new Vector2(300,100),22,false);view.detailDescription.fontSizeMin=16;
        view.detailLevel=Text("Level",left,new Vector2(0,-166),new Vector2(285,25),21,false);
        view.detailRequirements=Text("Requirements",left,new Vector2(0,-205),new Vector2(300,49),17,false);
        view.purchaseButton=Button(classes,"Upgrade",null,new Vector2(-700,-384),new Vector2(330,66),26);
        view.purchaseText=view.purchaseButton.GetComponentInChildren<TMP_Text>();
        var right=Rect("ClassDetails",content,new Vector2(700,-37),new Vector2(374,671));
        Card(right,catalog.classes[0].pattern,new Vector2(374,671),out view.classPattern);
        view.classEmblem=Medallion(right,catalog.classes[0],new Vector2(0,329),268);
        view.classNameplate=Image("NamePlate",right,S("Choose class/title_pyro.png"),new Vector2(0,166),new Vector2(312,55));
        view.classTitle=Text("ClassName",view.classNameplate.transform,Vector2.zero,new Vector2(278,45),31,true);
        Image("Divider",right,decor,new Vector2(0,97),new Vector2(260,20));
        view.classDescription=Text("Description",right,new Vector2(0,-2),new Vector2(283,152),25,false);
        Image("DividerBottom",right,decor,new Vector2(0,-114),new Vector2(260,20));
        view.resetButton=Button(right,"Reset","classes.reset",new Vector2(0,-196),new Vector2(309,61),26);
        view.resetButton.image.color=new Color(1,.82f,.82f);
        var refundPlate=Image("RefundPlate",right,S("Choose class/title_pyro.png"),new Vector2(0,-263),new Vector2(272,49));
        view.refundText=Text("Refund",refundPlate.transform,Vector2.zero,new Vector2(241,38),21,false);
        view.resetHint=Text("ResetHint",right,new Vector2(0,-307),new Vector2(310,28),16,false);
        var gold=Image("GoldPlate",classes,S("Choose class/title_geo.png"),new Vector2(-700,-457),new Vector2(348,62));
        Image("CoinMedallion",gold.transform,S("NEW money&shard/icon_money_2.png"),new Vector2(-132,0),new Vector2(82,98));
        Image("GoldIcon",gold.transform,S("NEW popup/icon_money.png"),new Vector2(-132,0),new Vector2(67,67)).preserveAspect=true;
        view.goldText=Text("Gold",gold.transform,new Vector2(27,0),new Vector2(212,46),34,true);
        view.lockedBackdrop=content.gameObject.AddComponent<ClassLockedBackdrop>();
        view.lockedBackdrop.blurShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/ClassLockedBlur.shader");
        var locked=Rect("LockedClass",content,Vector2.zero,new Vector2(1920,770));view.lockedRoot=locked.gameObject;view.lockedBackdrop.foreground=locked;
        var shade=Image("Shade",locked,null,Vector2.zero,new Vector2(1830,796));shade.color=new Color(0,0,0,.62f);shade.raycastTarget=true;
        Image("Panel",locked,S("NG+ warning/base.png"),new Vector2(0,48),new Vector2(750,360));
        view.lockedTitle=Text("LockedTitle",locked,new Vector2(0,85),new Vector2(657,144),35,false);
        view.unlockButton=Button(locked,"Unlock",null,new Vector2(0,-64),new Vector2(431,78),34);
        view.unlockButton.image.color=new Color(1,.75f,.75f);
        view.unlockText=view.unlockButton.GetComponentInChildren<TMP_Text>();
        view.unlockText.rectTransform.sizeDelta=new Vector2(295,55);view.unlockText.rectTransform.anchoredPosition=new Vector2(-25,0);
        Image("KeyIcon",view.unlockButton.transform,catalog.runeKey.icon,new Vector2(146,0),new Vector2(44,53)).preserveAspect=true;
        var keyPlate=Image("KeyCountPlate",locked,S("Class level up/key_back.png"),new Vector2(0,-191),new Vector2(338,80));
        view.keyCount=Text("KeyCount",keyPlate.transform,Vector2.zero,new Vector2(288,52),26,false);
        BuildAttacks(main,catalog);
        BuildOverlays(composition);
        choices.gameObject.SetActive(false);main.gameObject.SetActive(true);view.attacksRoot.SetActive(false);view.lockedRoot.SetActive(false);
        view.confirmation.SetActive(false);view.tooltip.gameObject.SetActive(false);view.choiceReveal.gameObject.SetActive(false);
        view.sideMenu.transform.SetAsLastSibling();view.confirmation.transform.SetAsLastSibling();view.choiceReveal.transform.SetAsLastSibling();
        // Keep the window in the scene and its menu connected to the shared prefab.
        var installer=Object.FindFirstObjectByType<Reflex.GameSceneInstaller>(FindObjectsInactive.Include);
        var fields=new SerializedObject(installer);fields.FindProperty("_classCatalog").objectReferenceValue=catalog;fields.FindProperty("_classesView").objectReferenceValue=view;fields.ApplyModifiedPropertiesWithoutUndo();
        var hub=Object.FindFirstObjectByType<HubView>(FindObjectsInactive.Include);var hubFields=new SerializedObject(hub);
        var shrine=hub.GetComponentsInChildren<HubFeatureButtonView>(true).First(b=>b.name=="ShrineButton");
        hubFields.FindProperty("_shrineButton").objectReferenceValue=shrine;hubFields.ApplyModifiedPropertiesWithoutUndo();
        var localizer=shrine.GetComponentsInChildren<UnityEngine.Localization.Components.LocalizeStringEvent>(true).FirstOrDefault();
        if(localizer!=null) {localizer.StringReference=new UnityEngine.Localization.LocalizedString("LocalesTable","classes.shrine");EditorUtility.SetDirty(localizer);}
        var backdrop=Object.FindFirstObjectByType<MenuBackdropView>(FindObjectsInactive.Include);
        backdrop.GetComponent<Canvas>().overrideSorting=true;
        backdrop.GetComponent<Canvas>().sortingOrder=99;
        shrine.Button.interactable=true;
        EditorSceneManager.MarkSceneDirty(installer.gameObject.scene);EditorSceneManager.SaveScene(installer.gameObject.scene);AssetDatabase.SaveAssets();
        return "Class window and side menu created as scene objects in GameScene.";
    }
    static void BuildAttacks(Transform main,ClassCatalog catalog)
    {
        var root=Rect("Attacks",main,Vector2.zero,new Vector2(1920,1080));view.attacksRoot=root.gameObject;
        view.attackPrefab=CreateAttackPrefab();view.attackColumns=new ClassAttackColumn[3];
        view.typeIcons=new[]{S("Class choise attack/sword.png"),S("Class choise attack/gear.png"),S("Class choise attack/explosion.png")};
        string[] types={"manual","automatic","special"};
        for(int i=0;i<3;i++)
        {
            float x=(i-1)*452;
            Title(root,"classes."+types[i],new Vector2(x,365),new Vector2(295,66),27);
            Image("AttackSigil",root,catalog.classes[0].sigil,new Vector2(x,-7),new Vector2(800,800)).color=new Color(1,1,1,.14f);
            var slot=Rect("Slot_"+types[i],root,new Vector2(x,253),new Vector2(160,140));
            var slotBack=Image("SlotBackground",slot,S("Class choise attack/type_attak_base.png"),Vector2.zero,new Vector2(134,134));slotBack.raycastTarget=true;
            var drop=slot.gameObject.AddComponent<ClassAttackDropZone>();drop.menu=view;drop.slot=(AttackSlot)i;
            var empty=Rect("Empty",slot,Vector2.zero,new Vector2(150,135));
            Image("Type",empty,view.typeIcons[i],Vector2.zero,new Vector2(75,75)).preserveAspect=true;
            var list=Rect("List_"+types[i],root,new Vector2(x,-164),new Vector2(178,664));
            var listFrame=Image("Frame",list,CreateListFrame(),Vector2.zero,new Vector2(178,664));listFrame.type=UnityEngine.UI.Image.Type.Sliced;listFrame.pixelsPerUnitMultiplier=2.1f;
            var viewport=Rect("Viewport",list,Vector2.zero,new Vector2(145,638));
            var bg=viewport.gameObject.AddComponent<Image>();bg.color=Color.clear;bg.raycastTarget=true;
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=Rect("Content",viewport,Vector2.zero,new Vector2(145,646));content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,646);
            var emptyCells=new GameObject[5];
            for(int row=0;row<5;row++)
            {
                var cell=Image("EmptyCell",content,S("Class choise attack/rama_slitoe.png"),new Vector2(0,-71-row*126),new Vector2(137,131));
                cell.rectTransform.anchorMin=cell.rectTransform.anchorMax=new Vector2(.5f,1);
                emptyCells[row]=cell.gameObject;
            }
            var scroll=list.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=42;
            var listDrop=list.gameObject.AddComponent<ClassAttackDropZone>();listDrop.menu=view;listDrop.slot=(AttackSlot)i;listDrop.unequip=true;
            var listEmpty=Label("EmptyList",root,"classes.empty_list",new Vector2(x,-521),new Vector2(380,27),16);
            view.attackColumns[i]=new ClassAttackColumn{equipped=slot,emptySlot=empty.gameObject,content=content,emptyList=listEmpty.gameObject,emptyCells=emptyCells};
        }
    }
    static Sprite CreateListFrame()
    {
        var path="Assets/SO/Classes/AttackListFrame.asset";
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null){var so=new SerializedObject(existing);var border=so.FindProperty("m_Border");if(border!=null){border.vector4Value=new Vector4(96,96,96,96);so.ApplyModifiedPropertiesWithoutUndo();}return existing;}
        var source=S("Class choise attack/rama_slitoe.png");
        var sprite=UnityEngine.Sprite.Create(source.texture,source.rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(96,96,96,96));
        AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static void BuildOverlays(Transform root)
    {
        var tip=Rect("AttackTooltip",root,Vector2.zero,new Vector2(400,285));view.tooltip=tip;
        var group=tip.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;
        Image("Body",tip,frame,Vector2.zero,new Vector2(400,285));
        view.tooltipHeader=Image("HeaderTint",tip,null,new Vector2(0,100),new Vector2(370,61));
        view.tooltipEmblem=Image("Emblem",tip,null,new Vector2(-150,99),new Vector2(51,54));
        view.tooltipType=Image("Type",tip,null,new Vector2(157,99),new Vector2(32,32));
        view.tooltipTitle=Text("Name",tip,new Vector2(0,99),new Vector2(240,54),23,true);
        view.tooltipDescription=Text("Description",tip,new Vector2(0,-27),new Vector2(350,168),21,false);
        var confirm=Rect("ResetConfirmation",root,Vector2.zero,new Vector2(1920,1080));view.confirmation=confirm.gameObject;
        var shade=Image("Shade",confirm,null,Vector2.zero,new Vector2(1920,1080));shade.color=new Color(0,0,0,.88f);shade.raycastTarget=true;
        Image("Panel",confirm,frame,Vector2.zero,new Vector2(820,460));
        view.confirmationText=Text("Text",confirm,new Vector2(0,48),new Vector2(722,305),29,false);
        view.confirmButton=Button(confirm,"Confirm","classes.confirm",new Vector2(-180,-150),new Vector2(310,72),28);
        view.cancelButton=Button(confirm,"Cancel","classes.cancel",new Vector2(180,-150),new Vector2(310,72),28);
        var modalCanvas=confirm.gameObject.AddComponent<Canvas>();modalCanvas.overrideSorting=true;modalCanvas.sortingOrder=120;confirm.gameObject.AddComponent<GraphicRaycaster>();
        var reveal=Rect("ClassReveal",root,Vector2.zero,new Vector2(1920,1080));view.choiceReveal=reveal.gameObject.AddComponent<CanvasGroup>();
        var revealShade=Image("Shade",reveal,null,Vector2.zero,new Vector2(1920,1080));revealShade.color=new Color(0,0,0,.95f);revealShade.raycastTarget=true;
        var revealCanvas=reveal.gameObject.AddComponent<Canvas>();revealCanvas.overrideSorting=true;revealCanvas.sortingOrder=121;reveal.gameObject.AddComponent<GraphicRaycaster>();
        view.revealEmblem=Medallion(reveal,null,new Vector2(0,50),330);
        view.revealTitle=Text("Title",reveal,new Vector2(0,-225),new Vector2(750,100),55,true);
    }
    static ClassNodeView CreateNodePrefab()
    {
        var root=Rect("ClassNode",null,Vector2.zero,new Vector2(132,136));var node=root.gameObject.AddComponent<ClassNodeView>();
        node.highlight=Image("Highlight",root,S("NEW icon/red_amb_back.png"),Vector2.zero,new Vector2(153,157));
        var frameImage=Image("Frame",root,tile,Vector2.zero,new Vector2(132,136));frameImage.raycastTarget=true;
        Image("Swirl",root,S("NEW icon/red.png"),Vector2.zero,new Vector2(95,95));
        node.icon=Image("Icon",root,null,Vector2.zero,new Vector2(94,94));node.icon.preserveAspect=true;
        node.lockIcon=Image("Lock",root,S("NEW icon/lock.png"),Vector2.zero,new Vector2(49,57)).gameObject;
        node.levelText=Text("Level",root,new Vector2(0,-48),new Vector2(95,24),22,false);
        node.nameText=Text("Name",root,Vector2.zero,new Vector2(150,40),20,false);node.nameText.gameObject.SetActive(false);
        node.button=root.gameObject.AddComponent<Button>();node.button.targetGraphic=frameImage;
        var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;
        var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefabs+"/ClassNode.prefab").GetComponent<ClassNodeView>();Object.DestroyImmediate(root.gameObject);return prefab;
    }
    static ClassAttackItemView CreateAttackPrefab()
    {
        var root=Rect("AttackItem",null,Vector2.zero,new Vector2(139,126));var item=root.gameObject.AddComponent<ClassAttackItemView>();
        var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
        Image("Frame",root,S("Class choise attack/rama_slitoe.png"),Vector2.zero,new Vector2(137,131));
        item.tint=Image("Tint",root,S("NEW icon/red_amb_back.png"),Vector2.zero,new Vector2(114,114));
        item.icon=Image("Icon",root,null,Vector2.zero,new Vector2(91,91));item.icon.preserveAspect=true;
        item.title=Text("Name",root,new Vector2(0,-55),new Vector2(220,31),20,false);item.title.gameObject.SetActive(false);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefabs+"/AttackItem.prefab").GetComponent<ClassAttackItemView>();Object.DestroyImmediate(root.gameObject);return prefab;
    }
    static Image Card(Transform parent,Sprite pattern,Vector2 size,out Image patternImage)
    {
        var back=Image("Frame",parent,frame,Vector2.zero,size);
        patternImage=Image("Pattern",parent,pattern,new Vector2(0,-3),size-new Vector2(32,32));
        return back;
    }
    static ClassMedallionView Medallion(Transform parent,ClassDefinition definition,Vector2 position,float size)
    {
        var root=Rect("Medallion",parent,position,new Vector2(size,size));
        var medallion=root.gameObject.AddComponent<ClassMedallionView>();
        medallion.halo=Image("Halo",root,null,new Vector2(0,size*.065f),new Vector2(size*1.06f,size*1.14f));
        Image("Metal",root,S("Choose class/pyro_bottom_4.png"),new Vector2(0,size*.065f),new Vector2(size,size*1.084f));
        medallion.aura=Image("Aura",root,null,Vector2.zero,new Vector2(size*.80f,size*.80f));
        medallion.runes=Image("Runes",root,null,Vector2.zero,new Vector2(size*.82f,size*.83f));
        medallion.sigil=Image("Sigil",root,null,Vector2.zero,new Vector2(size*1.20f,size*1.20f));
        medallion.element=(ClassEmblemImage)Image("Emblem",root,null,Vector2.zero,new Vector2(size*.51f,size*.51f));
        if(definition!=null)medallion.Bind(definition);
        return medallion;
    }
    static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform));go.layer=5;var r=(RectTransform)go.transform;r.SetParent(parent,false);r.sizeDelta=size;r.anchoredPosition=pos;return r;
    }
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    static Image Image(string name,Transform parent,Sprite sprite,Vector2 pos,Vector2 size)
    {
        var go=Rect(name,parent,pos,size).gameObject;
        Image image=name=="Emblem" ? go.AddComponent<ClassEmblemImage>() : go.AddComponent<Image>();image.sprite=sprite;image.useSpriteMesh=true;image.raycastTarget=false;return image;
    }
    static TMP_Text Text(string name,Transform parent,Vector2 pos,Vector2 size,float fontSize,bool heading)
    {
        var text=Rect(name,parent,pos,size).gameObject.AddComponent<TextMeshProUGUI>();text.font=heading?header:body;text.fontSize=fontSize;
        if(heading)text.fontStyle=FontStyles.UpperCase;
        text.enableAutoSizing=true;text.fontSizeMax=fontSize;text.fontSizeMin=fontSize*.78f;text.color=ink;
        text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Ellipsis;
        text.fontSharedMaterial=textMaterial;
        return text;
    }
    static TMP_Text Label(string name,Transform parent,string key,Vector2 pos,Vector2 size,float fontSize)
    {
        var text=Text(name,parent,pos,size,fontSize,false);text.gameObject.AddComponent<ClassLocalizedLabel>().key=key;return text;
    }
    static TMP_Text Title(Transform parent,string key,Vector2 pos,Vector2 size,float fontSize)
    {
        var back=Image("Title",parent,title,pos,size);var t=Text("TitleText",back.transform,Vector2.zero,size-new Vector2(65,20),fontSize,true);t.gameObject.AddComponent<ClassLocalizedLabel>().key=key;return t;
    }
    static Button Button(Transform parent,string name,string key,Vector2 pos,Vector2 size,float fontSize)
    {
        var image=Image(name,parent,name.EndsWith("Tab")?title:S("Choose class/title_pyro.png"),pos,size);image.raycastTarget=true;var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
        var text=Text("Text",image.transform,Vector2.zero,size-new Vector2(48,14),fontSize,true);if(key!=null)text.gameObject.AddComponent<ClassLocalizedLabel>().key=key;
        return button;
    }
    static Sprite S(string path)=>LoadSprite(Art+path);
    static Sprite LoadSprite(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
}
