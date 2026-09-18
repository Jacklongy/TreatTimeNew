using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Changing this to a random powerUp - Increase speed of score tick. - Delete all items on Grid - Feed the dog!
// all with a different sprite.

public class Coin : Item
{
    public GameObject CoinPopUp;
    public GameObject particles;

    void Start()
    {
        OnGrid = true;
        gameObject.layer = 7;
    }

    public override void Grabbed()
    {
        FindObjectOfType<SoundManagerScript>().Play("Ding");

        Instantiate(CoinPopUp, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity);

        Instantiate(particles, gameObject.transform.position, gameObject.transform.rotation);

        // report through the central economy manager instead of a direct DogFeeder reference
        //SessionEconomyManager.Instance?.AddCoins(1);

        Destroy(gameObject);
    }

}
