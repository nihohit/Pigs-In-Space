using Assets.Scripts.Base;
using Assets.Scripts.UnityBase;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapScene
{
    /// <summary>
    ///  Script wrapper for shots moving across the screen
    /// </summary>
    public class ShotScript : MonoBehaviour
    {
        #region fields

        private bool m_started = false;
        private Vector2 m_startingPoint;
        private Vector2 m_movementSpeed;
        private bool m_piercing;
        private List<SquareScript> m_hitSquares;
        private Vector2 m_endPoint;
        private int m_effectSize;

        public IEnumerable<SquareScript> HitSquares { get { return m_hitSquares; } }

        #endregion fields

        // initialize the shot with its information
        public void Init(SquareScript to, SquareScript from, float range, bool piercing, int effectSize, float shotSpread)
        {
            m_effectSize = effectSize;
            m_hitSquares = new List<SquareScript>();
            m_piercing = piercing;
            m_startingPoint = from.transform.position;
            transform.position = m_startingPoint;
            m_started = true;

            // find the point the shot will end in
            m_endPoint = RaycastToTarget(to.transform.position, m_startingPoint, range, shotSpread);

            // find the angle the shot goes in
            var differenceVector = m_endPoint - m_startingPoint;
            var angle = Vector2.Angle(new Vector2(1, 0), differenceVector);
            if (differenceVector.y < 0)
            {
                angle = -angle;
            }
            this.gameObject.transform.Rotate(new Vector3(0, 0, angle));

            // set the shot's speed by the normal of its direction vector
            m_movementSpeed = differenceVector.normalized / 4;

            // destroy the shot after 1 second
            Destroy(gameObject, 1f);
        }

        // find the spot the shot will end on, and on the way, find all the squares affected by it.
        private Vector2 RaycastToTarget(Vector2 to, Vector2 from, float range, float shotSpread)
        {
            var layerMask = 1 << LayerMask.NameToLayer("Ground");

            // return all colliders that the ray passes through
            var rayHits = Physics2D.RaycastAll(from, GetDirection(to, from, range, shotSpread), range, layerMask);
            foreach (var rayHit in rayHits)
            {
                var square = rayHit.collider.gameObject.GetComponent<SquareScript>();
                if (BlockingSquare(square))
                {
                    m_hitSquares.Add(square);

                    // piercing weapons can go through anything but blocking terrain
                    if (square.TraversingCondition == Traversability.Blocking || !m_piercing)
                    {
                        return rayHit.point;
                    }
                }
            }

            var lastHit = rayHits[rayHits.Length - 1];
            m_hitSquares.Add(lastHit.collider.gameObject.GetComponent<SquareScript>());
            return lastHit.point;
        }

        // find the shot's direction, when adjusting for its spread
        private Vector2 GetDirection(Vector2 to, Vector2 from, float range, float shotSpread)
        {
            var direction = to - from;
            return (direction.normalized * range) + GetShotSpread(shotSpread);
        }

        // add randomness to the shot
        private Vector2 GetShotSpread(float shotSpread)
        {
            return new Vector2((float)Randomiser.NextDouble(-shotSpread, shotSpread), (float)Randomiser.NextDouble(-shotSpread, shotSpread));
        }

        // This only checks if the square in general blocks, it ignores whether a shot is piercing
        private bool BlockingSquare(SquareScript square)
        {
            return square != null &&
                (square.TraversingCondition == Traversability.Blocking ||
                // block if its not the starting position and the square is occupied
                (square.OccupyingEntity != null &&
                (Vector2)square.transform.position != m_startingPoint));
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_started) return;
            transform.position = (Vector2)m_movementSpeed + (Vector2)transform.position;

            foreach (var square in HitSquares)
            {
                if (square.GetComponent<BoxCollider2D>().Bounds().Overlaps(this.GetComponent<BoxCollider2D>().Bounds()))
                {
                    foreach (var affectedSquare in square.MultiplyBySize(m_effectSize))
                    {
                        var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), affectedSquare.transform.position, Quaternion.identity));
                        UnityEngine.Object.Destroy(pow, 0.3f);
                    }
                }
            }
            if (this.GetComponent<BoxCollider2D>().Bounds().Contains(m_endPoint))
            {
                Destroy(gameObject);
            }
        }
    }
}