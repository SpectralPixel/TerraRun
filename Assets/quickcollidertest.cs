using System.Collections;
using UnityEngine;

public class quickcollidertest : MonoBehaviour
{

    public Transform player;

    private void Start()
    {
        StartCoroutine(SetYToPlayer());
        InvokeRepeating("SetXToPlayer", 1f, 0.1f);
    }

    IEnumerator SetYToPlayer()
    {
        yield return new WaitForSeconds(1);
        transform.position = new Vector3(player.position.x, player.position.y - 0.75f);
        yield return null;
    }

    void SetXToPlayer()
    {
        transform.position = new Vector3(player.position.x, transform.position.y);
    }
}
