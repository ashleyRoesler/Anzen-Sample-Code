using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ReimEnt.Anzen.Skills.Modules {

    [AddComponentMenu(AnzenConstants.SkillBehaviorModulesComponentPath + "/VFX"), HideMonoScript]
    public class VFXModule : SkillBehaviorModule {

        [Tooltip("The default location is the user of the skill")]
        public List<VFXModuleParameters> StartVFX;

        [Tooltip("Follows the weapon in use. Cannot be used with non-weapon skills.")]
        public List<VFXModuleParameters> TrailVFX;
        private List<SpawnedVisualEffect> _spawnedTrailVFX = new List<SpawnedVisualEffect>();

        [Tooltip("The default location is whatever was hit")]
        public List<VFXModuleParameters> HitVFX;

        [ShowIfComponent(typeof(ChargingModule))]
        public List<VFXModuleParameters> ChargingVFX;
        private List<SpawnedVisualEffect> _spawnedChargingVFX = new List<SpawnedVisualEffect>();
        private float _chargeDuration = 0f;

        [ShowIfComponent(typeof(ChargingModule))]
        public List<VFXModuleParameters> ChargeReleaseVFX;
        private List<SpawnedVisualEffect> _spawnedReleaseVFX = new List<SpawnedVisualEffect>();

        [Tooltip("If true, hit vfx will always play even if no target was hit. Make sure to add the \"Play Hit VFX\" event to the animation module.")]
        public bool AlwaysPlayHitVFX = false;

        protected override void OnInitializing() {
            if(TryGetComponent(out DamageModule module))
                module.Applied += DamageModule_Applied;

            if (TryGetComponent(out ChargingModule charging)) {
                charging.Began += Charging_Began;
                charging.Released += Charging_Released;

                _chargeDuration = charging.MaximumTime;
            }

            SkillBehavior.Began += SkillBehavior_Began;
            SkillBehavior.Ended += SkillBehavior_Ended;

            _spawnedTrailVFX.Clear();
            _spawnedChargingVFX.Clear();
            _spawnedReleaseVFX.Clear();

            Random.InitState(System.DateTime.Now.Millisecond);
        }

        private void SkillBehavior_Began() {
            Fire(StartVFX, User.transform.position);
            FireTrail();            
        }

        private void SkillBehavior_Ended() {
            StopVFX(_spawnedReleaseVFX);
            StopVFX(_spawnedTrailVFX);
            StopVFX(_spawnedChargingVFX);
        }

        private void Charging_Began() {
            Fire(ChargingVFX, User.transform.position, true);
        }

        private void Charging_Released() {
            StopVFX(_spawnedChargingVFX);
            Fire(ChargeReleaseVFX, User.transform.position, false, true);
        }

        private void DamageModule_Applied(float amount, Character victim) {
            StopVFX(_spawnedTrailVFX);
            StopVFX(_spawnedReleaseVFX);

            if (!AlwaysPlayHitVFX)
                Fire(HitVFX, victim.transform.position);
        }

        private void Fire(List<VFXModuleParameters> parameters, Vector3 defaultPosition, bool isCharging = false, bool isReleasing = false) {
            foreach(var p in parameters) {
                float random = Random.Range(0f, 100f);
                if (random <= p.Chance) {
                    Play(p, GetLocation(p, defaultPosition), isCharging, isReleasing);
                }
            }
        }

        private void FireTrail() {
            foreach(var p in TrailVFX) {
                float random = Random.Range(0f, 100f);
                if (random <= p.Chance) {

                    // attach trail vfx to weapon socket
                    var instance = Pooling.Spawn(p.Prefab);

                    if (User.View is HumanCharacterView hcv)
                        hcv.AttachWeaponTrailVFX(instance, SkillBehavior.Hand);
                    else
                        User.View.AttachTrailVFX(instance, p.SocketBone);

                    // play effects
                    var ve = instance.GetComponent<SpawnedVisualEffect>();
                    ve.Play();

                    _spawnedTrailVFX.Add(ve);
                }
            }
        }

        private Vector3 GetLocation(VFXModuleParameters parameters, Vector3 defaultLocation) {
            switch (parameters.Location) {
                case VFXModuleParametersLocation.Skill:
                    return SkillBehavior.transform.position;
                case VFXModuleParametersLocation.User:
                    return User.transform.position;
                case VFXModuleParametersLocation.Other:
                    return parameters.Other.position;
                default: return defaultLocation;
            }
        }

        private Transform GetParent(VFXModuleParameters parameters) {
            switch (parameters.Parent) {
                case VFXModuleParametersParent.None:
                    return null;
                case VFXModuleParametersParent.Skill:
                    return SkillBehavior.transform;
                case VFXModuleParametersParent.User:
                    return User.transform;
                default:
                    return null;
            }
        }

        private void Play(VFXModuleParameters parameters, Vector3 position, bool isCharging, bool isReleasing) {
            
            var instance = Pooling.Spawn(parameters.Prefab, position, Quaternion.identity);

            Transform parent = GetParent(parameters);
            if (parent != null) {
                instance.transform.SetParent(parent, true);
                instance.transform.localRotation = Quaternion.identity;
            }

            // play effects
            var ve = instance.GetComponent<SpawnedVisualEffect>();
            ve.Play(isCharging, _chargeDuration);

            if (isCharging)
                _spawnedChargingVFX.Add(ve);

            if (isReleasing)
                _spawnedReleaseVFX.Add(ve);
        }

        [DesignerMethod("Play Hit VFX")]
        public void Designer_PlayHitVFX() {
            StopVFX(_spawnedTrailVFX);
            Fire(HitVFX, User.transform.position);
        }

        /// <summary>
        /// Used to stop looping effects and weapon trails
        /// </summary>
        /// <param name="visualEffects"></param>
        private void StopVFX(List<SpawnedVisualEffect> visualEffects) {
            foreach (SpawnedVisualEffect ve in visualEffects)
                ve.Stop();
        }
    }

    [System.Serializable]
    public class VFXModuleParameters {

        [Required, AssetsOnly]
        [NeedsComponent(typeof(SpawnedVisualEffect))]
        public GameObject Prefab;

        [ShowIf(nameof(Prefab))]
        [Range(0f, 100f), SuffixLabel("%")]
        public float Chance = 100f;

        [Tooltip("Default is the safest choice if you're not sure")]
        public VFXModuleParametersLocation Location = VFXModuleParametersLocation.Default;

        [ChildGameObjectsOnly]
        [ShowIf("Location", VFXModuleParametersLocation.Other)]
        public Transform Other;

        [Tooltip("The object that the VFX will follow")]
        public VFXModuleParametersParent Parent = VFXModuleParametersParent.None;

        [Space, Tooltip("Only use for non-human skills (User has a CharacterView and not a HumanCharacterView).")]
        public bool ManuallyOverrideTrailVFXSocket = false;

        [ShowIf(nameof(ManuallyOverrideTrailVFXSocket)), Tooltip("If this skill doesn't have a weapon, which socket do you want to attach the trail VFX to?")]
        public SocketBoneType SocketBone;
    }

    public enum VFXModuleParametersLocation {
        Default, User, Skill, Other
    }

    public enum VFXModuleParametersParent {
        None, User, Skill
    }
}