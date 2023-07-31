using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Infiniscryption.Curses.Components
{
    public class Orbiter : ManagedBehaviour
    {
        private const float FULL_ROTATION = 360f;

        public bool Reversed { get; set; }

        public float OrbitSpeed { get; set; } = 0.333f;

        private float _orbitRadius = 1f;
        public float OrbitRadius 
        { 
            get
            {
                return _orbitRadius;
            }
            set
            {
                _orbitRadius = value;
                _cos0vector = Vector3.Normalize(_cos0vector) * value;
                _sin0vector = Vector3.Normalize(_sin0vector) * value;
            }
        }

        public float ThetaOffset { get; set; } = 0f;

        public bool RandomThetaOffset { get; set; }

        private Vector3 _rotationAxis = Vector3.Cross(Vector3.forward, Vector3.up);

        private Vector3 _cos0vector = Vector3.forward;
        public Vector3 Cos0Vector 
        { 
            get
            {
                return _cos0vector;
            }
            set
            {
                _cos0vector = Vector3.Normalize(value) * this.OrbitRadius;
                _rotationAxis = Vector3.Normalize(Vector3.Cross(value, Sin0Vector));
            }
        }

        private Vector3 _sin0vector = Vector3.up;
        public Vector3 Sin0Vector 
        { 
            get
            {
                return _sin0vector;
            }
            set
            {
                _sin0vector = Vector3.Normalize(value) * this.OrbitRadius;
                _rotationAxis = Vector3.Normalize(Vector3.Cross(Cos0Vector, _sin0vector));
            }
        }

        private float Theta { get; set; }

        private void Awake()
        {
            if (this.RandomThetaOffset)
                this.Theta = UnityEngine.Random.Range(0, FULL_ROTATION);
            else
                this.Theta = this.ThetaOffset;

            this.SetLocation();
        }

        private void Start()
        {
            base.enabled = true;
        }

        public void StartFromBeginning()
        {
            base.enabled = true;
            if (this.RandomThetaOffset)
                this.Theta = UnityEngine.Random.Range(0, FULL_ROTATION);
            else
                this.Theta = this.ThetaOffset;
        }

        // Token: 0x060000BF RID: 191 RVA: 0x00006A4F File Offset: 0x00004C4F
        public void Stop()
        {
            base.enabled = false;
        }

        private void SetLocation()
        {
            Vector3 rotVec = Quaternion.AngleAxis(this.Theta, this._rotationAxis) * this.Cos0Vector;
            this.transform.localPosition = rotVec;
        }

        public override void ManagedUpdate()
        {
            if (this.enabled)
            {
                float num = this.UpdateWhenPaused ? Time.unscaledDeltaTime : Time.deltaTime;
                this.Theta += FULL_ROTATION * this.OrbitSpeed * num * (this.Reversed ? -1f : 1f);
                while (this.Theta > FULL_ROTATION)
                    this.Theta -= FULL_ROTATION;
                while (this.Theta < -FULL_ROTATION)
                    this.Theta += FULL_ROTATION;

                this.SetLocation();
            }
        }
    }

}