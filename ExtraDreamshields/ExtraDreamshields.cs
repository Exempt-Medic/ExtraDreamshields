using Modding;
using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;

namespace ExtraDreamshields
{
    public class ExtraDreamshieldsMod : Mod
    {
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

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public ExtraDreamshieldsMod() : base("ExtraDreamshields")
        {
            _instance = this;
        }

        public override void Initialize()
        {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += MoreDreamshields;
            On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter += SlashAnims;

            Log("Initialized");
        }

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
                var spawn = self.GetFsmAction<SpawnObjectFromGlobalPool>("Spawn", 2);

                self.AddFsmAction("Spawn", spawn);
                self.AddFsmAction("Spawn", new Rotate()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetFsmGameObjectVariable("Shield")
                    },
                    vector = new Vector3(0, 0, 0),
                    xAngle = 0,
                    yAngle = 0,
                    zAngle = 90,
                    space = Space.World,
                    perSecond = false,
                    everyFrame = false,
                    lateUpdate = false,
                    fixedUpdate = false
                });

                self.AddFsmAction("Spawn", spawn);
                self.AddFsmAction("Spawn", new Rotate()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetFsmGameObjectVariable("Shield")
                    },
                    vector = new Vector3(0, 0, 0),
                    xAngle = 0,
                    yAngle = 0,
                    zAngle = 180,
                    space = Space.World,
                    perSecond = false,
                    everyFrame = false,
                    lateUpdate = false,
                    fixedUpdate = false
                });

                self.AddFsmAction("Spawn", spawn);
                self.AddFsmAction("Spawn", new Rotate()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                        GameObject = self.GetFsmGameObjectVariable("Shield")
                    },
                    vector = new Vector3(0, 0, 0),
                    xAngle = 0,
                    yAngle = 0,
                    zAngle = 270,
                    space = Space.World,
                    perSecond = false,
                    everyFrame = false,
                    lateUpdate = false,
                    fixedUpdate = false
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
    }
}
