using Assets.scripts.UnityBase;
using UnityEngine;

/// <summary>
///  Script wrapper for shots moving across the screen
/// </summary>
public class ShotScript : MonoBehaviour
{
    #region fields

    private bool m_started = false;
    private Vector2 m_startingPoint;
    private Vector2 m_movementSpeed;
    public SquareScript HitSquare { get; private set; }

    #endregion fields

    public void Init(SquareScript to, SquareScript from, string name, float range)
    {
        m_startingPoint = from.transform.position;
        HitSquare = to;
        var endPoint = RaycastToTarget(to.transform.position, m_startingPoint, range);
        transform.position = m_startingPoint;
        m_started = true;
        var differenceVector = endPoint - m_startingPoint;
        var angle = Vector2.Angle(new Vector2(1, 0), differenceVector);
        if (differenceVector.y < 0)
        {
            angle = -angle;
        }
        this.gameObject.transform.Rotate(new Vector3(0, 0, angle));
        m_movementSpeed = differenceVector.normalized / 4;
        Destroy(gameObject, 1f);
    }

    private Vector2 RaycastToTarget(Vector2 to, Vector2 from, float range)
    {
        var layerMask = 1 << LayerMask.NameToLayer("Ground");

        // return all colliders that the ray passes through
        var rayHits = Physics2D.RaycastAll(from, to - from, range, layerMask);
        foreach (var rayHit in rayHits)
        {
            if (Blocking(rayHit.collider.gameObject.GetComponent<SquareScript>()) ||
                (to.x == rayHit.collider.gameObject.GetComponent<SquareScript>().transform.position.x && 
                 to.y == rayHit.collider.gameObject.GetComponent<SquareScript>().transform.position.y)) 
            {
                HitSquare = rayHit.collider.gameObject.GetComponent<SquareScript>();
                return rayHit.point;
            }
        }

        var lastHit = rayHits[rayHits.Length-1];
        HitSquare = lastHit.collider.gameObject.GetComponent<SquareScript>();
        return lastHit.point;
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

        if (HitSquare.GetComponent<BoxCollider2D>().Bounds().Overlaps(this.GetComponent<BoxCollider2D>().Bounds()))
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        if (HitSquare != null)
        {
            var pow = ((GameObject)MonoBehaviour.Instantiate(Resources.Load("pow"), transform.position, Quaternion.identity));
            UnityEngine.Object.Destroy(pow, 0.3f);
        }
        Destroy(gameObject);
    }
}