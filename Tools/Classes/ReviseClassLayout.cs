using System.Linq;
using Core.Classes;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization.Tables;

// One-time revision of the initial test layout. Does not change IDs, prices, or player progress.
public static class ReviseClassLayout
{
    public static string Run()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<ClassCatalog>("Assets/SO/Classes/ClassCatalog.asset");
        foreach(var c in catalog.classes)
        {
            var art=c.id=="fire"?"pyro":c.id=="ice"?"kryo":"geo";
            c.emblemAura=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/NEW/Choose class/"+art+"_bottom_2.png").OfType<Sprite>().First();
            // The imported sheet splits the rune circle into individual glyphs; use the complete texture.
            var runesPath="Assets/SO/Classes/"+c.id+"/EmblemRunes.asset";
            c.emblemRunes=AssetDatabase.LoadAssetAtPath<Sprite>(runesPath);
            if(c.emblemRunes==null)
            {
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/NEW/Choose class/"+art+"_bottom_3.png");
                c.emblemRunes=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                AssetDatabase.CreateAsset(c.emblemRunes,runesPath);
            }
            c.emblemHalo=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/NEW/Choose class/"+art+"_bottom_5.png").OfType<Sprite>().First();
            EditorUtility.SetDirty(c);
            var attacks=c.nodes.Where(n=>n.reward==ClassNodeReward.UnlockAttack).ToArray();
            Vector2[] positions={new(0,-230),new(-215,-42),new(215,-42),new(-160,155),new(160,155)};
            for(int i=0;i<attacks.Length;i++){attacks[i].position=positions[i];attacks[i].visualScale=1;}
            attacks[3].requirements=new[]{new ClassNodeRequirement{node=attacks[1],level=1}};
            attacks[4].requirements=new[]{new ClassNodeRequirement{node=attacks[2],level=1}};
            foreach(var n in c.nodes)
            {
                if(n.reward==ClassNodeReward.RuneKey)
                {
                    n.position=new Vector2(0,302);n.visualScale=.82f;
                    n.requirements=new[]{new ClassNodeRequirement{node=attacks[3],level=1},new ClassNodeRequirement{node=attacks[4],level=1}};
                }
                if(n.reward is ClassNodeReward.UpgradeAttack or ClassNodeReward.UpgradeClassEffect)
                {
                    n.visualScale=.64f;
                    n.position=n.parameterId switch
                    {
                        "radius"=>new Vector2(-160,-278),
                        "effect_duration"=>new Vector2(160,-278),
                        "thickness"=>new Vector2(-363,-20),
                        "damage_2"=>new Vector2(363,-20),
                        "cooldown_2"=>new Vector2(360,85),
                        "damage_3"=>new Vector2(-363,189),
                        "cooldown_3"=>new Vector2(-282,288),
                        "cooldown_4"=>new Vector2(282,288),
                        "effect_radius"=>new Vector2(0,-351),
                        _=>n.position
                    };
                }
                EditorUtility.SetDirty(n);
            }
        }
        foreach(var lang in new[]{"ru-RU","en-US","zh"})
        {
            var table=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/Locales/LocalesTable_"+lang+".asset");
            table.AddEntry("classes.before_introduction",lang=="ru-RU"?"Завершите все уровни первой локации, чтобы выбрать свой первый класс.":"Complete every level of the first location to choose your first class.");
            EditorUtility.SetDirty(table);EditorUtility.SetDirty(table.SharedData);
        }
        AssetDatabase.SaveAssets();return "Branched node layout, grouped upgrades, prerequisites and pre-introduction Shrine text updated.";
    }
}
