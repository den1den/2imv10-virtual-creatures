using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VirtualCreatures
{
    public class TreeNodeUI : MonoBehaviour {

        private IList<TreeNodeUI> childNodes = new List<TreeNodeUI>();

        private Morphology morphology;

        public Vector2 position;

        public RectTransform _rectTransform;
        // Use this for initialization
        void Start()
        {
            _rectTransform = this.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(Morphology morphology)
        {
            this.morphology = morphology;
        }

        public static TreeNodeUI createTreeNodeUI(Morphology morphology, TreeUI parent)
        {
            GameObject treeNodeUI = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GUI/TreeNodeUI.prefab"));

            TreeNodeUI treeNodeUIScript = (TreeNodeUI)treeNodeUI.GetComponent<TreeNodeUI>();

            treeNodeUI.transform.SetParent(parent.transform);
            treeNodeUI.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            treeNodeUIScript.Initialize(morphology);

            return treeNodeUIScript;
        }

        public void removeTreeNodeUI()
        {

        }

        public void updatePosition()
        {
            Debug.Log("Hi");
            this.transform.position = new Vector3(position.x, position.y, this.transform.position.z);
        }

        public void addChildNode(TreeNodeUI node)
        {
            childNodes.Add(node);
        }
        
        public Vector3 getPosition()
        {
            return position;
        }

        public void setPosition(float x, float y)
        {
            position = new Vector2(x, y);
            _rectTransform = this.GetComponent<RectTransform>(); 
            _rectTransform.position = new Vector3(x, y, _rectTransform.position.z);
        }

        public bool isMorphology(Morphology morphology)
        {
            return this.morphology.Equals(morphology);
        }

        public IList<TreeNodeUI> getChildNodes()
        {
            return childNodes;
        }
    }
}
