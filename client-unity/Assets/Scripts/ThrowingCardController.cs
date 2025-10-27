using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;

#nullable enable
public class ThrowingCardController : MonoBehaviour
{
    public uint Id;
    public Identity OwnerIdentity;
    public uint MatchId;
    public Vector3 TargetPosition;

    public void Initalize(ThrowingCard ThrowingCard)
    {
        Id = ThrowingCard.Id;
        OwnerIdentity = ThrowingCard.OwnerIdentity;
        MatchId = ThrowingCard.MatchId;
        transform.position = ThrowingCard.Position;
        TargetPosition = ThrowingCard.Position;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, ThrowingCard.Direction);
    }

    void Start()
    {
        GameManager.Conn.Db.ThrowingCards.OnUpdate += HandleThrowingCardUpdate;
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPosition, k);
    }

    public void HandleThrowingCardUpdate(EventContext context, ThrowingCard oldThrowingCard, ThrowingCard newThrowingCard)
    {
        if (Id != newThrowingCard.Id) return;
        TargetPosition = newThrowingCard.Position;
    }
    
    public void Delete(EventContext context)
    {
        Destroy(gameObject);
    }
}