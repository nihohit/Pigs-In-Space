﻿using UnityEngine;
using System.Collections;

    /// <summary>
    ///  Script wrapper for shots moving across the screen
    /// </summary>
    public class LaserScript : MonoBehaviour
    {
        private bool m_started = false;
        private Vector2 m_movementFraction;
        private Vector2 m_endPoint;

        public void Init(Vector2 to, Vector2 from, string name)
        {
            transform.position = from;
            m_endPoint = to;
            m_started = true;
            var differenceVector = to - from;
            var angle = Vector2.Angle(new Vector2(1, 0), differenceVector);
            if (differenceVector.y < 0)
            {
                angle = -angle;
            }
            this.gameObject.transform.Rotate(new Vector3(0, 0, angle));
            m_movementFraction = differenceVector / 30;
            //TacticalState.TextureManager.UpdateEffectTexture(name, this.GetComponent<SpriteRenderer>());
        }

        // Use this for initialization
        private void Start()
        {
            Destroy(this.gameObject, 0.6f);
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_started) return;
            transform.position = (Vector2)m_movementFraction + (Vector2)transform.position;
            if (m_endPoint.Equals(transform.position))
            {
                Destroy(this);
            }
        }
    }
