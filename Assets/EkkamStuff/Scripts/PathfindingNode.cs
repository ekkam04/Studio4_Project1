﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ekkam
{
    public class PathfindingNode : MonoBehaviour
    {
        public Vector2Int gridPosition;
        public List<PathfindingNode> neighbours = new List<PathfindingNode>();
        public bool isBlocked;
        private int gCost;
        public TMP_Text gCostText;
        private Color initialColor;

        void Awake()
        {
            initialColor = GetComponent<MeshRenderer>().material.color;
        }
        public int GCost
        {
            get
            {
                return gCost;
            }
            set
            {
                gCost = value;
                gCostText.text = gCost.ToString();
                fCost = gCost + hCost;
                fCostText.text = fCost.ToString();
            }
        }
        private int hCost;
        public TMP_Text hCostText;
        public int HCost
        {
            get
            {
                return hCost;
            }
            set
            {
                hCost = value;
                hCostText.text = hCost.ToString();
                fCost = gCost + hCost;
                fCostText.text = fCost.ToString();
            }
        }
        private int fCost;
        public TMP_Text fCostText;
        public int FCost
        {
            get
            {
                return fCost;
            }
        }
        public PathfindingNode Parent;
        
        private GameObject occupant;
        private string occupantName;
        public TMP_Text occupantText;
        public GameObject Occupant
        {
            get
            {
                return occupant;
            }
            set
            {
                occupant = value;
                if (occupant != null)
                {
                    occupantName = occupant.name;
                    occupantText.text = occupantName;
                }
                else
                {
                    occupantName = "Empty";
                    occupantText.text = occupantName;
                }
            }
        }
        
        public void SetColor(Color color)
        {
            if (GetComponent<MeshRenderer>().material.color == initialColor)
            {
                GetComponent<MeshRenderer>().material.color = color;
            }
        }    
        public void SetPathColor(Color color)
        {
            GetComponent<MeshRenderer>().material.color = color;
        }
        
        public void ResetColor()
        {
            GetComponent<MeshRenderer>().material.color = initialColor;
        }
    }
}