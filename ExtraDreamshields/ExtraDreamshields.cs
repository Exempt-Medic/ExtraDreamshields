using Modding;
using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using Satchel.BetterMenus;
using Satchel;

namespace ExtraDreamshields
{
    #region Menu
    public static class ModMenu
    {
        private static Menu? MenuRef;
        public static MenuScreen CreateModMenu(MenuScreen modlistmenu)
        {
            MenuRef ??= new Menu("Extra Dreamshields Options", new Element[]
            {
                new CustomSlider(
                        "Number of Dreamshields",
                        f => ExtraDreamshieldsMod.LS.dreamshields = (int)f,
                        () => ExtraDreamshieldsMod.LS.dreamshields,
                        1f,
                        10f,
                        true)
            });

            return MenuRef.GetMenuScreen(modlistmenu);
        }
    }
    #endregion
    public class ExtraDreamshieldsMod : Mod, ICustomMenuMod, ILocalSettings<LocalSettings>
    {
        #region Boilerplate
        private static ExtraDreamshieldsMod? _instance;

        internal static ExtraDreamshieldsMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(ExtraDreamshieldsMod)} was never constructed");
                }
                return _instance;
            }
        }

        public static LocalSettings LS { get; private set; } = new();
        public void OnLoadLocal(LocalSettings s) => LS = s;
        public LocalSettings OnSaveLocal() => LS;
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenu(modListMenu);
        public bool ToggleButtonInsideMenu => false;
        public ExtraDreamshieldsMod() : base("ExtraDreamshields")
        {
            _instance = this;
        }
        #endregion

        #region Init
        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += MoreDreamshields;
            On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter += SlashAnims;

            Log("Initialized");
        }
        #endregion

        #region Changes
        private void SlashAnims(On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.orig_OnEnter orig, Tk2dPlayAnimation self)
        {
            if (self.Fsm.GameObject.name == "Shield" && self.Fsm.Name == "Shield Hit" && self.State.Name == "Slash Anim")
            {
                PlayMakerFSM.BroadcastEvent("DREAMSHIELD SLASH");
            }

            orig(self);
        }

        private void MoreDreamshields(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            if (self.gameObject.name == "Charm Effects" && self.FsmName == "Spawn Orbit Shield")
            {
                var spawner = self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 2);
                self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 2).Enabled = false;

                self.AddCustomAction("Spawn", () =>
                {
                    for (int i = 1; i <= LS.dreamshields; i++)
                    {
                        spawner.OnEnter();
                        var shield = self.GetFsmGameObjectVariable("Shield").Value.gameObject;
                        shield.transform.Rotate(0, 0, (360 / LS.dreamshields) * i, 0);
                    }
                });
            }

            else if (self.gameObject.transform.parent?.name == "Orbit Shield(Clone)" && self.gameObject.name == "Shield" && self.FsmName == "Shield Hit")
            {
                self.AddFsmTransition("Idle", "DREAMSHIELD SLASH", "Slash Anim");
                self.AddFsmState("Send Break");
                self.ChangeFsmTransition("Tink", "FINISHED", "Send Break");
                self.ChangeFsmTransition("G Parent?", "FINISHED", "Send Break");
                self.AddFsmGlobalTransitions("BREAK DREAMSHIELD", "Break");
                self.AddFsmAction("Send Break", new SendEvent()
                {
                    eventTarget = new FsmEventTarget()
                    {
                        target = FsmEventTarget.EventTarget.BroadcastAll,
                    },
                    sendEvent = FsmEvent.GetFsmEvent("BREAK DREAMSHIELD"),
                    delay = 0,
                    everyFrame = false,
                });
            }
        }
        #endregion
    }
    #region Settings
    public class LocalSettings
    {
        public int dreamshields = 1;
    }
    #endregion
}
