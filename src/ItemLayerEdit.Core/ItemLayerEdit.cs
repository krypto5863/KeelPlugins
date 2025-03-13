using BepInEx;
using BepInEx.Configuration;
using KeelPlugins;
using KKAPI.Studio.SaveLoad;
using Studio;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

[assembly: System.Reflection.AssemblyFileVersion(ItemLayerEdit.Koikatu.ItemLayerEdit.Version)]

namespace ItemLayerEdit.Koikatu
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    [BepInProcess(Constants.StudioProcessName)]
    [BepInPlugin(GUID, "Item Layer Edit", Version)]
    public class ItemLayerEdit : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.itemlayeredit";
        public const string Version = "1.1.2." + BuildNumber.Version;

        private ConfigEntry<KeyboardShortcut> ChangeLayer { get; set; }
        private ConfigEntry<KeyboardShortcut> ChangeLayerOfChildren { get; set; }

		private void Start()
        {
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneDataController>(GUID);

            ChangeLayer = Config.Bind("General", "Change layer", new KeyboardShortcut(KeyCode.V), "Toggle the selected objects between character and map layers");
            ChangeLayerOfChildren = Config.Bind("General", "Change layer of children", new KeyboardShortcut(KeyCode.V, KeyCode.LeftControl), "Toggle the selected object and it's children between character and map layers");
		}

        private void Update()
        {
            if(ChangeLayer.Value.IsDown())
            {
                var layer = GetSelectedObjectLayer();
                if(layer == 11)
                    SetSelectedObjectLayer(10);
                else if(layer == 10)
                    SetSelectedObjectLayer(11);
            }

            if (ChangeLayerOfChildren.Value.IsDown())
            {
	            var layer = GetSelectedObjectLayer();
	            if (layer == 11)
		            SetSelectedObjectAndChildrenLayer(10);
	            else
		            SetSelectedObjectAndChildrenLayer(11);
            }
		}

        private static void SetSelectedObjectLayer(int layer)
        {
			var targetObjects = Studio.Studio.GetSelectObjectCtrl().OfType<OCIItem>().Select(x => x.objectItem);

			foreach (var targetObject in targetObjects)
            {
	            SetObjectLayer(layer, targetObject);
            }
        }
        private static void SetSelectedObjectAndChildrenLayer(int layer)
        {
	        var targetObjects = Studio.Studio.GetSelectObjectCtrl().OfType<OCIFolder>().Select(x => x.objectItem)
		        .Concat(Studio.Studio.GetSelectObjectCtrl().OfType<OCIItem>().Select(x => x.objectItem));

	        foreach (var targetObject in targetObjects)
	        {
		        SetObjectLayerAndChildrenLayer(layer, targetObject);
	        }
        }

		private static void SetObjectLayer(int layer, GameObject targetObject)
        {
	        if(targetObject.AddComponentIfNotExist<LayerDataContainer>(out var data))
		        data.DefaultLayer = targetObject.layer;

	        targetObject.SetAllLayers(layer);
        }

        private static void SetObjectLayerAndChildrenLayer(int layer, GameObject targetObject)
        {
	        SetObjectLayer(layer, targetObject);

	        foreach (Transform child in targetObject.transform)
	        {
                SetObjectLayerAndChildrenLayer(layer, child.gameObject);
	        }
		}

        private static int GetSelectedObjectLayer()
        {
            var targetObject = Studio.Studio.GetSelectObjectCtrl().OfType<OCIItem>().Select(x => x.objectItem).FirstOrDefault() 
                               ?? Studio.Studio.GetSelectObjectCtrl().OfType<OCIFolder>().Select(x => x.objectItem).FirstOrDefault();
            return targetObject != null ? targetObject.layer : -1;
        }
    }
}
