using UnityEngine;
using System.Collections;

    /// <summary>
    ///  Script wrapper for shots moving across the screen
    /// </summary>
    public class LaserScript : MonoBehaviour
    {
        private bool m_started = false;
        private Vector2 m_startingPoint;
        private Vector2 m_movementSpeed;
        private Vector2 m_endPoint;
        private double m_damage;
        private SquareScript m_targetSquare;

        public void Init(Vector2 to, Vector2 from, string name, float minDamage, float maxDamage)
        {
            m_damage = UnityEngine.Random.Range(minDamage, maxDamage);
            m_startingPoint = from;
            var endPoint = RaycastToTarget(to, from);
            transform.position = from;
            m_endPoint = endPoint;
            m_started = true;
            var differenceVector = endPoint - from;
            var angle = Vector2.Angle(new Vector2(1, 0), differenceVector);
            if (differenceVector.y < 0)
            {
                angle = -angle;
            }
            this.gameObject.transform.Rotate(new Vector3(0, 0, angle));
            m_movementSpeed = differenceVector.normalized / 4;
        }

        private Vector2 RaycastToTarget(Vector2 to, Vector2 from)
        {
            var layerMask = 1 << LayerMask.NameToLayer("Ground");

            // return all colliders that the ray passes through
            var rayHits = Physics2D.RaycastAll(from, to -from, to.Distance(from), layerMask);
            foreach (var rayHit in rayHits)
            {
                if (Blocking(rayHit.collider.gameObject.GetComponent<SquareScript>()))
                {
                    m_targetSquare = rayHit.collider.gameObject.GetComponent<SquareScript>();
                    return rayHit.point;
                }
            }
            return to;
        }

        private bool Blocking(SquareScript square)
        {
            return square != null && 
                (square.TraversingCondition == Traversability.Blocking || 
                (square.OccupyingEntity != null && (Vector2)square.transform.position != m_startingPoint));
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_started) return;
            transform.position = (Vector2)m_movementSpeed + (Vector2)transform.position;
            if(GetComponent<BoxCollider2D>().Bounds().Contains(m_endPoint))
            {
                Destroy();
            }
        }

        private void Destroy()
        {
            if(m_targetSquare != null)
            {
                var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), transform.position, Quaternion.identity));
                UnityEngine.Object.Destroy(pow, 0.3f);  
                if(m_targetSquare.OccupyingEntity != null)
                {
                    m_targetSquare.OccupyingEntity.Damage(m_damage);
                }
            }
            Destroy(gameObject);
        }
    }
