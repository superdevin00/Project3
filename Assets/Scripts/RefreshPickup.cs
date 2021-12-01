using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshPickup : MonoBehaviour
{
    private float rotate = 0;
    [SerializeField] float rotationSpeed = 4.0f;
    [SerializeField] GameObject artGroup;
    [SerializeField] Collider collider;
    PlayerController player;
    [SerializeField] float powerupDuration;

    private bool isActive = true;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isActive)
        {
            rotate = rotationSpeed;
            transform.Rotate(0, rotate, 0);
            collider.enabled = true;
            artGroup.SetActive(true);
        }
        else
        {
            collider.enabled = false;
            artGroup.SetActive(false);
            transform.Rotate(0, rotate, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isActive = false;
        player.setCurrentState(PlayerController.playerState.normal);
        StartCoroutine(PowerupTimer(powerupDuration));
    }

    private IEnumerator PowerupTimer(float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isActive = true;
    }


}
