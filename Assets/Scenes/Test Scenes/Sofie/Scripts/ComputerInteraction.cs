using UnityEngine;

public class ComputerInteraction : MonoBehaviour
{
    public GameObject pressEText;
    public GameObject playerCameraObj;
    public GameObject computerCameraObj;
    public MonoBehaviour playerMovementScript; // reference til dit movement script

    private bool isNear = false;
    private bool usingComputer = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressEText.SetActive(true);
            isNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressEText.SetActive(false);
            isNear = false;
        }
    }

    void Update()
    {
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            // Skjul "Press E"
            pressEText.SetActive(false);

            if (!usingComputer)
            {
                // Skift til computer kamera
                playerCameraObj.SetActive(false);
                computerCameraObj.SetActive(true);
                usingComputer = true;

                // Lås player movement
                if (playerMovementScript != null)
                    playerMovementScript.enabled = false;

                // Lås cursor op
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Skift tilbage til player kamera
                playerCameraObj.SetActive(true);
                computerCameraObj.SetActive(false);
                usingComputer = false;

                // Lås player movement op
                if (playerMovementScript != null)
                    playerMovementScript.enabled = true;

                // Lås cursor tilbage til FPS
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Vis "Press E" igen hvis stadig inde i trigger
                if (isNear)
                    pressEText.SetActive(true);
            }
        }
    }
}
