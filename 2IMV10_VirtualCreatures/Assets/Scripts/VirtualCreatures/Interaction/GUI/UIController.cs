using UnityEngine;
using System.Collections;

namespace VirtualCreatures
{
    public class UIController : MonoBehaviour {

        private GameObject treeUI_GO;

        private CameraController cameraController;
        // Use this for initialization
        void Start() {
            TreeUI treeUI = GetComponentInChildren<TreeUI>();
            treeUI_GO = treeUI.gameObject;
            treeUI_GO.SetActive(false);

            cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

        }

        // Update is called once per frame
        void Update() {
            // If button X is pressed
            if (Input.GetKeyDown(KeyCode.X))
                treeUI_GO.SetActive(!treeUI_GO.active);

            if (Input.GetKeyDown(KeyCode.Escape))
                treeUI_GO.SetActive(false);

            if (Input.GetKeyDown(KeyCode.Q))
                cameraController.changeMode(CameraController.CameraMode.Free);

            if (Input.GetKeyDown(KeyCode.E))
                cameraController.changeMode(CameraController.CameraMode.Top);

        }

        public void TreeUIOnClick()
        {
            treeUI_GO.SetActive(!treeUI_GO.active);
        }


    }
}
