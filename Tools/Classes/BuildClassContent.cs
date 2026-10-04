using System;
using System.IO;
using System.Linq;
using Core.Classes;
using Core.Items;
using Core.Gameplay.Dungeon;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Utils;

public static class BuildClassContent
{
    const string Root = "Assets/SO/Classes";
    const string Art = "Assets/Art/UI/NEW/";
    static StringTable ru, en, zh;
    public static string Run()
    {
        Directory.CreateDirectory(Root);
        ru = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/Locales/LocalesTable_ru-RU.asset");
        en = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/Locales/LocalesTable_en-US.asset");
        zh = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/Locales/LocalesTable_zh.asset");
        Labels();
        var catalog = AssetDatabase.LoadAssetAtPath<ClassCatalog>(Root + "/ClassCatalog.asset");
        if (catalog != null) { AssetDatabase.SaveAssets(); return "Existing class content preserved."; }
        catalog = Create<ClassCatalog>(Root + "/ClassCatalog.asset");
        catalog.introductionDungeon = AssetDatabase.LoadAssetAtPath<DungeonList>("Assets/SO/Dungeons/DungeonList.asset").GetFirstPlayable();
        var key = Create<ItemDefinition>(Root + "/RuneKey.asset");
        key.itemId = "rune_key"; key.displayName = "Rune Key"; key.category = ItemCategory.Misc;
        key.stackable = true; key.maxStackSize = 99; key.sellPrice = 100;
        key.icon = Sprite("Class level up/key.png");
        key.localizedName = Text("classes.rune_key", "Рунный ключ", "Rune Key");
        key.localizedDescription = Text("classes.rune_key.description", "Открывает новый класс в святилище.", "Unlocks a class at the shrine.");
        catalog.runeKey = key;
        var itemCatalog = AssetDatabase.FindAssets("t:ItemCatalog").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ItemCatalog>).First();
        var items = new SerializedObject(itemCatalog); var list = items.FindProperty("_items");
        list.InsertArrayElementAtIndex(list.arraySize); list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = key;
        items.ApplyModifiedPropertiesWithoutUndo();
        var basic = Create<AttackDefinition>(Root + "/BasicStrike.asset");
        basic.id = "basic_strike"; basic.slot = AttackSlot.Manual; basic.icon = Sprite("Class choise attack/sword.png");
        basic.displayName = Text("classes.basic", "Обычный удар", "Basic Strike");
        basic.description = Text("classes.basic.description", "Обычная ручная атака без стихии.", "A manual attack without an element.");
        catalog.defaultManualAttack = basic;
        string[][] titles = {
            new[]{"Огненный удар","Огненное кольцо","Взрыв","Дождь огня","Огненное торнадо"},
            new[]{"Ледяной удар","Ледяное кольцо","Сосульчатая ловушка","Ледяная кора","Метель"},
            new[]{"Земляной удар","Земляное кольцо","Каменная статуя","Каменная глыба","Землетрясение"}
        };
        string[][] english = {
            new[]{"Fire Strike","Fire Ring","Explosion","Fire Rain","Fire Tornado"},
            new[]{"Ice Strike","Ice Ring","Icicle Trap","Ice Crust","Blizzard"},
            new[]{"Earth Strike","Earth Ring","Stone Statue","Boulder","Earthquake"}
        };
        string[][] descriptions = {
            new[]{"Наносит урон и поджигает врагов в части зоны ручной атаки.","Кольцо на границе зоны периодически поджигает врагов.","Взрыв наносит урон и поджигает врагов вокруг точки удара.","Огненные шары падают на клетки поля в шахматном порядке, наносят урон и поджигают врагов.","Поджигает всех врагов на поле."},
            new[]{"Наносит урон и замораживает врагов в части зоны ручной атаки.","Кольцо на границе зоны периодически замораживает врагов.","Сосульки вырастают возле центра зоны. При касании наносят урон и замораживают врагов.","Ледяная поверхность наносит урон и замораживает стоящих на ней врагов. Сохраняется до конца уровня.","Замораживает всех врагов на поле."},
            new[]{"Наносит урон и накладывает заземление в части зоны ручной атаки.","Песчаное кольцо на границе зоны периодически накладывает заземление.","Враги в зоне превращаются в статуи, которые затем взрываются и накладывают заземление.","Глыба падает в центр поля, наносит урон, накладывает заземление и разлетается на снаряды.","Накладывает заземление на всех врагов на поле."}
        };
        string[][] descriptionsEn = {
            new[]{"Damages and burns enemies within part of the manual attack area.","A ring at the edge of your area periodically burns enemies.","An explosion damages and burns nearby enemies.","Fireballs fall across alternating tiles, damaging and burning enemies.","Burns every enemy on the field."},
            new[]{"Damages and freezes enemies within part of the manual attack area.","A ring at the edge of your area periodically freezes enemies.","Icicles grow near the centre, damaging and freezing enemies on contact.","An icy surface damages and freezes enemies standing on it until the level ends.","Freezes every enemy on the field."},
            new[]{"Damages and grounds enemies within part of the manual attack area.","A sandy ring at the edge of your area periodically grounds enemies.","Enemies in your area turn to stone, then explode and ground nearby foes.","A boulder hits the centre, damages and grounds enemies, then shatters into projectiles.","Grounds every enemy on the field."}
        };
        string[] ids = {"fire","ice","earth"}, artIds = {"pyro","kryo","geo"};
        string[] names = {"Пиромант","Криократ","Геоарх"}, namesEn = {"Pyromancer","Cryomancer","Geoarch"};
        string[] classDesc = {"Обрати пещеры в доменную печь. Поджигай врагов, окружай их пламенем и обрушивай на орду дождь огня.","Замораживай толпы врагов и управляй полем боя. Создавай ледяные ловушки и выигрывай драгоценные секунды.","Обрушивай на врагов силу подземных глубин. Заземление передаёт часть полученного ими урона соседним противникам."};
        string[] classDescEn = {"Turn the caves into a furnace. Burn your enemies, surround them with flames and unleash a rain of fire.","Freeze the horde and control the battlefield. Create icy traps and buy precious seconds.","Command the strength of the depths. Grounded enemies send part of the damage they receive into nearby foes."};
        var colors = new[]{new Color(1,.22f,.06f), new Color(.12f,.6f,1), new Color(.8f,.47f,.13f)};
        catalog.classes = new ClassDefinition[3];
        for (int c = 0; c < 3; c++)
        {
            string folder = Root + "/" + ids[c]; Directory.CreateDirectory(folder);
            var def = Create<ClassDefinition>(folder + "/Class.asset"); catalog.classes[c] = def;
            def.id = ids[c]; def.color = colors[c]; def.unlockKeyCost = 1;
            def.displayName = Text("classes."+ids[c]+".name", names[c], namesEn[c]);
            def.description = Text("classes."+ids[c]+".description", classDesc[c], classDescEn[c]);
            def.banner = Sprite("Class level up/"+artIds[c]+"_small.png");
            def.pattern = Sprite("Choose class/"+artIds[c]+"_pattern.png");
            def.sigil = Sprite("Choose class/"+artIds[c]+"_bottom_1.png");
            def.emblemAura = Sprite("Choose class/"+artIds[c]+"_bottom_2.png");
            var runesTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Choose class/"+artIds[c]+"_bottom_3.png");
            def.emblemRunes=UnityEngine.Sprite.Create(runesTexture,new Rect(0,0,runesTexture.width,runesTexture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            AssetDatabase.CreateAsset(def.emblemRunes,folder+"/EmblemRunes.asset");
            def.emblemHalo = Sprite("Choose class/"+artIds[c]+"_bottom_5.png");
            var tex = def.banner.texture;
            var emblem = UnityEngine.Sprite.Create(tex, new Rect(0,0,Mathf.Min(355, tex.width),tex.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            AssetDatabase.CreateAsset(emblem, folder+"/Emblem.asset"); def.emblem = emblem;
            var nodes = new System.Collections.Generic.List<ClassNodeDefinition>();
            for(int a=0; a<5; a++)
            {
                var attack = Create<AttackDefinition>(folder+"/Attack"+a+".asset");
                attack.id = ids[c]+"_"+a; attack.slot = a==0 ? AttackSlot.Manual : a==1 ? AttackSlot.Automatic : AttackSlot.Special;
                attack.displayName = Text("classes."+attack.id+".name", titles[c][a], english[c][a]);
                attack.description = Text("classes."+attack.id+".description", descriptions[c][a], descriptionsEn[c][a]);
                attack.icon = FindAttackSprite(titles[c][a]); attack.preview = attack.icon;
                var node = Create<ClassNodeDefinition>(folder+"/Unlock"+a+".asset");
                node.id = ids[c]+"_unlock_"+a; node.displayName = attack.displayName; node.description = attack.description;
                node.icon = attack.icon; node.preview=attack.preview; node.reward=ClassNodeReward.UnlockAttack; node.attack=attack;
                node.maxLevel=1; node.goldCostPerLevel=new BigDouble[]{100*(a+1)};
                if(a>0) node.requirements = new[]{new ClassNodeRequirement{node=nodes[a<3?0:a-2],level=1}};
                node.position=new[]{new Vector2(0,-230),new Vector2(-215,-42),new Vector2(215,-42),new Vector2(-160,155),new Vector2(160,155)}[a];
                nodes.Add(node);
            }
            string[] suffixes = {"radius","thickness","damage_2","cooldown_2","damage_3","cooldown_3","cooldown_4","effect_duration"};
            string[] upgradeNames = {"Радиус удара","Толщина кольца","Сила: "+titles[c][2],"Перезарядка: "+titles[c][2],"Сила: "+titles[c][3],"Перезарядка: "+titles[c][3],"Перезарядка: "+titles[c][4],"Длительность эффекта"};
            string[] upgradeNamesEn = {"Strike radius","Ring thickness",english[c][2]+": damage",english[c][2]+": cooldown",english[c][3]+": damage",english[c][3]+": cooldown",english[c][4]+": cooldown","Effect duration"};
            int[] parents = {0,1,2,2,3,3,4,0};
            for(int n=0; n<suffixes.Length; n++)
            {
                var node=Create<ClassNodeDefinition>(folder+"/Upgrade_"+suffixes[n]+".asset");
                node.id=ids[c]+"_"+suffixes[n]; node.displayName=Text("classes."+node.id+".name",upgradeNames[n],upgradeNamesEn[n]);
                string ruDescription=n==0?"Увеличивает радиус стихийного удара внутри зоны игрока.":n==1?"Увеличивает толщину стихийного кольца.":n==7?"Увеличивает длительность эффекта стихии.":suffixes[n].StartsWith("damage")?"Увеличивает урон особой атаки.":"Ускоряет перезарядку особой атаки.";
                string enDescription=n==0?"Increases the elemental strike radius within your area.":n==1?"Increases the thickness of the elemental ring.":n==7?"Increases elemental effect duration.":suffixes[n].StartsWith("damage")?"Increases special attack damage.":"Improves special attack cooldown.";
                node.description=Text("classes."+node.id+".description",ruDescription,enDescription);
                node.icon=nodes[parents[n]].icon; node.attack=nodes[parents[n]].attack;
                node.reward=n==7?ClassNodeReward.UpgradeClassEffect:ClassNodeReward.UpgradeAttack;
                node.visualScale=.64f;
                node.position=new[]{new Vector2(-160,-278),new Vector2(-363,-20),new Vector2(363,-20),new Vector2(360,85),new Vector2(-363,189),new Vector2(-282,288),new Vector2(282,288),new Vector2(160,-278)}[n];
                node.parameterId=suffixes[n]; node.maxLevel=3; node.goldCostPerLevel=new BigDouble[]{150,300,600}; node.valuesPerLevel=new[]{.1f,.1f,.1f};
                node.requirements=new[]{new ClassNodeRequirement{node=nodes[parents[n]],level=1}};
                nodes.Add(node);
            }
            if(c==2)
            {
                var node=Create<ClassNodeDefinition>(folder+"/Upgrade_effect_radius.asset"); node.id="earth_effect_radius";
                node.displayName=Text("classes.earth_effect_radius.name","Радиус заземления","Grounding radius");
                node.description=Text("classes.earth_effect_radius.description","Увеличивает область волны заземления.","Increases the area of the grounding wave.");
                node.visualScale=.64f;node.position=new Vector2(0,-351);
                node.reward=ClassNodeReward.UpgradeClassEffect; node.icon=nodes[4].icon; node.parameterId="effect_radius";
                node.maxLevel=3; node.goldCostPerLevel=new BigDouble[]{150,300,600}; node.valuesPerLevel=new[]{1f,1f,1f};
                node.requirements=new[]{new ClassNodeRequirement{node=nodes[0],level=1}}; nodes.Add(node);
            }
            var keyNode=Create<ClassNodeDefinition>(folder+"/RuneKey.asset"); keyNode.id=ids[c]+"_rune_key";
            keyNode.displayName=key.localizedName; keyNode.description=Text("classes.key_node.description","Даёт рунный ключ для открытия другого класса. Можно получить снова после сброса и повторной прокачки этой ветки.","Grants a rune key to unlock another class. Available again after resetting and rebuilding this class.");
            keyNode.icon=key.icon; keyNode.reward=ClassNodeReward.RuneKey; keyNode.maxLevel=1; keyNode.goldCostPerLevel=new BigDouble[]{500};
            keyNode.position=new Vector2(0,302);keyNode.visualScale=.82f;
            keyNode.requirements=new[]{new ClassNodeRequirement{node=nodes[3],level=1},new ClassNodeRequirement{node=nodes[4],level=1}}; nodes.Add(keyNode);
            foreach(var node in nodes) EditorUtility.SetDirty(node);
            def.nodes=nodes.ToArray(); EditorUtility.SetDirty(def);
        }
        foreach(var path in AssetDatabase.FindAssets("",new[]{Root}).Select(AssetDatabase.GUIDToAssetPath))
            foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(path)) if(obj!=null) EditorUtility.SetDirty(obj);
        EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(key); EditorUtility.SetDirty(basic);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        return "Created class catalog, 15 elemental attacks, configurable nodes and inventory rune key. Test balance only.";
    }
    public static Sprite Sprite(string path) => AssetDatabase.LoadAllAssetsAtPath(Art+path).OfType<Sprite>().First();
    static Sprite FindAttackSprite(string name)
    {
        var path=Directory.GetFiles(Art+"Class level up","*.png").First(p=>Path.GetFileNameWithoutExtension(p).TrimEnd('_').Normalize()==name.Normalize());
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    }
    static T Create<T>(string path) where T:ScriptableObject {var obj=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(obj,path);return obj;}
    static LocalizedString Text(string key,string russian,string english)
    {
        ru.AddEntry(key,russian); en.AddEntry(key,english); zh.AddEntry(key,english);
        EditorUtility.SetDirty(ru);EditorUtility.SetDirty(en);EditorUtility.SetDirty(zh);EditorUtility.SetDirty(ru.SharedData);
        return new LocalizedString("LocalesTable",key);
    }
    static void Labels()
    {
        Text("classes.before_introduction","Завершите все уровни первой локации, чтобы выбрать свой первый класс.","Complete every level of the first location to choose your first class.");
        Text("classes.choose","Выберите свой класс","Choose your class");
        Text("classes.classes","Классы","Classes");Text("classes.attacks","Атаки","Attacks");
        Text("classes.manual","Ручная атака","Manual attack");Text("classes.automatic","Автоатака","Auto attack");Text("classes.special","Особая атака","Ultimate");
        Text("classes.choose_hint","Нажмите, чтобы выбрать","Click to choose");
        Text("classes.empty_slot","Перетащите атаку сюда","Drop an attack here");
        Text("classes.empty_list","Открывайте атаки\nв ветках классов","Unlock attacks\nin class branches");
        Text("classes.attacks_hint","Нажмите или перетащите атаку в слот. Чтобы снять — нажмите на неё или перенесите в список.","Click or drag an attack into its slot. Click an equipped attack or drag it back to the list to remove it.");
        Text("classes.locked","Откройте класс «{0}»\nс помощью рунного ключа","Unlock {0}\nwith a rune key");
        Text("classes.keys","Рунные ключи: {0}","Rune keys: {0}");Text("classes.unlock","Открыть за {0}","Unlock for {0}");
        Text("classes.reset","Сбросить класс","Reset class");Text("classes.refund","Возврат: {0} золота","Refund: {0} gold");
        Text("classes.last_class","Сначала откройте другой класс","Unlock another class before resetting");
        Text("classes.reset_confirm","Сбросить класс «{0}»?\n\nЕго улучшения и атаки будут сброшены, назначенные атаки сняты. Класс потребуется открыть заново.\n\nВы получите {1} золота.","Reset {0}?\n\nIts upgrades and attacks will be removed, and its equipped attacks cleared. You will need to unlock the class again.\n\nYou receive {1} gold.");
        Text("classes.confirm","Сбросить","Reset");Text("classes.cancel","Отмена","Cancel");
        Text("classes.level","Уровень {0} / {1}","Level {0} / {1}");Text("classes.requirement","Требуется: {0}, ур. {1}","Requires: {0}, lv. {1}");
        Text("classes.buy","Улучшить · {0}","Upgrade · {0}");Text("classes.max","Максимальный уровень","Maximum level");
        Text("classes.not_enough_gold","Недостаточно золота","Not enough gold");Text("classes.class_locked","Класс закрыт","Class locked");
        Text("classes.requirements_missing","Выполните требования","Requirements not met");Text("classes.not_configured","Улучшение недоступно","Upgrade unavailable");
        Text("classes.shrine","Святилище","Shrine");
    }
}
