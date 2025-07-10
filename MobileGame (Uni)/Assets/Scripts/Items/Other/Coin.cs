using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Item
{
    public GameObject CoinPopUp;
    public DogFeeder Feeder;
    public GameObject particles;

    void Start()
    {
        OnGrid = true;
        gameObject.layer = 7;
        Feeder = FindObjectOfType<DogFeeder>();

    }

    public override void Grabbed()
    {
        FindObjectOfType<SoundManagerScript>().Play("Ding");

        Instantiate(CoinPopUp, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity);

        Instantiate(particles, gameObject.transform.position, gameObject.transform.rotation);
        // add to coin count
        Feeder.coinCount += 1;

        Destroy(gameObject);
    }

}
