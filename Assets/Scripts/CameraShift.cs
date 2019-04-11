using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraShift : MonoBehaviour
{
    public GameObject virtualCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            virtualCamera.SetActive(true);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            virtualCamera.SetActive(false);
            // If any items are active on the board, disable them.
            if (Stats.active != null)
                Stats.active.gameObject.SetActive(false);
            // Add the room cleared score. 
            Stats.AddScore(50 * (Stats.ItemRoomCount + 1));
            // Add a score bonus based on the amount of health the player has on room clear.
            Stats.AddScore((int)(Stats.Hp * 30));
            if (Stats.PreviousHp == Stats.Hp)
                Stats.IncrementMultiplier(0.5f);
            else
            {
                // Only reset the multipler outside of leaving item rooms in the case of a HP up item being chosen.
                if (Stats.RoomCount % 5 != 0 && Stats.ItemRoomCount != 0)
                    Stats.IncrementMultiplier(0);
            }

            Stats.PreviousHp = Stats.Hp;
            Stats.LastPos = collision.transform.position;
            Stats.LastPos = new Vector2(Stats.LastPos.x, 0.5f + Mathf.Round((Stats.LastPos.y * 2) / 2) - 10.0f);
            // Delay to allow the camera to switch before resetting the scene to look like the room's filled...
            Invoke("SwitchRoom", 0.15f);
        }
    }

    private void SwitchRoom()
    {
        SceneManager.LoadScene("Runner");
    }

    //private void Update()
    //{
    //    if (PollVC)
    //    {
    //        if (!Cinemachine.CinemachineCore.Instance.IsLive(virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>()))
    //        {
    //            SceneManager.LoadScene("Runner");
    //        }
    //    }
        
    //}
}
