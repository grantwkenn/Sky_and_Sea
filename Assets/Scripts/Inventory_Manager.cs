﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Inventory_Manager : MonoBehaviour
{
    Quest_Manager qm;
    Scene_Persistence persistenceData;
    
    public Item emptyItem;

    public RectTransform inv;
    public RectTransform bar;

    Transform barSelector;
    Transform menuSelector;
    
    [SerializeField]
    Item[] items;
    Image[] menuImages;
    Image[] barImages;

    int inventorySize = 44;
    int rowSize = 11;


    int itemCount = 0;
    int emptyCount;
    int barSelection = 0;
    public int menuSelection = 0;

    int menuRow = 0;
    int menuCol = 0;
    int numRows = 4;
    int numCols = 11;

    bool inputBreak = true;

    Vector3[] barPositions;
    Vector3[] menuPositions;


    public Item_List itemList;

    AudioSource gmAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        qm = GetComponentInParent<Quest_Manager>();
        persistenceData = this.GetComponentInParent<Scene_Manager>().scenePersistence;

        inventorySize = persistenceData.inventorySize;

        items = new Item[inventorySize];


        for(int i=0; i<inventorySize; i++)
        {
            items[i] = persistenceData.inventory[i];  
            
            if (items[i] == null)
                items[i] = emptyItem;
        }

        menuImages = new Image[inventorySize];

        Transform[] rows = new RectTransform[4];
        for(int i=0; i<rows.Length; i++)
        {
            string s = "row" + i;
            rows[i] = inv.Find(s);
        }
        
        
        for(int i =0; i< rows.Length; i++)
        {
            //Find the row
            //string r = "row" + i;
            //Transform row = inv.Find(r);
            for(int j=0; j< numCols; j++)
            {
                string s = "Slot" + j;
                Transform slot = rows[i].Find(s);
                menuImages[(i * numCols) + j] = slot.GetComponentInChildren<Image>();
            }
        }

        barImages = new Image[numCols];
        
        for(int i=0; i< numCols; i++)
        {
            string s = "Slot" + i;
            Transform slot = bar.Find(s);
            barImages[i] = slot.GetComponentInChildren<Image>();
        }

        barPositions = new Vector3[numCols];
        for(int i=0; i< numCols; i++)
        {
            barPositions[i] = new Vector3(3 + i * 18, 3, 0);
        }

        menuPositions = new Vector3[numRows * numCols];
        for(int i=0; i< numRows; i++)
        {
            for(int j=0; j< numCols; j++)
            {
                float x = rows[i].transform.localPosition.x - 2;
                float y = rows[i].transform.localPosition.y - 2;
                menuPositions[(numCols * i) + j] = new Vector3(x + 18*j, y, 0);
             
            }
        }

        barSelector = bar.Find("Selection");
        menuSelector = inv.Find("Selection");

        gmAudioSource = GetComponentInParent<AudioSource>();

        emptyCount = inventorySize - itemCount;

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < items.Length; i++)
        {

            if (items[i].sprite != null)
            {
                menuImages[i].enabled = true;
                menuImages[i].sprite = items[i].sprite;
            }
            else
                menuImages[i].enabled = false;
        }

        for (int i=0; i< numCols; i++)
        {
            if (items[i].sprite != null)
            {
                barImages[i].enabled = true;
                barImages[i].sprite = items[i].sprite;
            }
            else
                barImages[i].enabled = false;
        }

        barSelector.transform.localPosition = barPositions[barSelection];
        menuSelector.transform.localPosition = menuPositions[menuSelection];

    }

    public bool inventoryFull()
    {
        return itemCount == inventorySize;
    }

    private void addItem(Item item)
    {
        //redundent
        if (inventoryFull()) return;

        if (items[barSelection] == emptyItem)
            items[barSelection] = item;
        else
        {
            for (int i = 0; i < inventorySize; i++)
            {
                if (items[i] == emptyItem)
                {
                    items[i] = item;
                    break;
                }
            }
        }
        itemCount++;
        emptyCount = inventorySize - itemCount;

        playSound();

        //notify the Quest Manager
        //qm.itemPickedUp(item.itemNo);
        qm.itemAddedOrRemoved(item.itemNo, 1);
              
    }

    public Item[] getSelectedRow()
    {
        Item[] firstRow = new Item[11];
        Array.Copy(items, 0, firstRow, 0, 11);
        
        return firstRow;

    }


    public Item getEmptyItem()
    {
        return emptyItem;
    }

    public void toggleSelection(int val)
    {
        barSelection += val;
        if (barSelection >= rowSize)
            barSelection = 0;
        else if (barSelection < 0)
            barSelection = rowSize - 1;
    }

    public void inputUpdate(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            inputBreak = true;
            return;
        }

        if (!inputBreak) return;

        inputBreak = false;

        if(input.x < 0) // Left
        {
            if(menuCol > 0)
            menuCol--;
        }
        else if (input.x > 0) // LEFT
        {
            if (menuCol < numCols - 1)
                menuCol++;
        }
            
        else if (input.y < 0) // DOWN
        {
            if (menuRow < numRows - 1)
                menuRow++;
        }
        else if (input.y > 0) // UP
        {
            if (menuRow > 0)
                menuRow--;
        }

        menuSelection = (menuRow * numCols) + menuCol;
    }

    public int getSelectionNumber()
    {
        return barSelection;
    }

    public Item getSelectedItem()
    {
        return items[barSelection];
    }

    public void discardSelection()
    {       
        items[barSelection] = emptyItem;
        itemCount--;
    }


    void playSound()
    {
        gmAudioSource.Play();
    }

    public bool enoughSpace(int number)
    {
        return emptyCount >= number;
    }

    //will need to fix for stackables...
    public bool offerItems(List<Item> items)
    {
        if (!enoughSpace(items.Count)) return false;

        foreach(Item i in items)
        {
            addItem(i);
        }


        return true;
    }

    public bool offerItem(Item item)
    {
        if (!enoughSpace(1)) return false;
        addItem(item);
        return true;
    }

    public bool removeObjectives(List<Gather_Objective> gathers)
    {
        bool result = true;
        
        foreach(Gather_Objective ob in gathers)
        {
            if (!removeItem(ob.getData().itemID, ob.getData().quantity)) result = false;
        }
               
        return result;
    }
    

    public bool removeItem(int itemID, int quantity)
    {
        //check each item and remove until quantity has been discarded.
        //if insufficient quantity, item will be converted to empty

        int quantityToRemove = quantity;
            
        //search every item for match, then subtract amount

        for(int i=0; i<items.Length; i++)
        {
            Item item = items[i];
            
            if (item.itemNo != itemID) continue;

            int itemQuantity = item.quantity;

            if (itemQuantity <= quantityToRemove)
            {
                quantityToRemove -= itemQuantity;

                qm.itemAddedOrRemoved(item.itemNo, 0 - itemQuantity);

                items[i] = emptyItem;
                Debug.Log("emptied");
            }
            else
            {
                quantityToRemove -= itemQuantity;
                
            }

            if (quantityToRemove == 0) return true;
        }

        qm.itemAddedOrRemoved(itemID, 0 - quantity);


        return quantityToRemove == 0;

     
    }

    public Item[] getItems()
    {
        return items;
    }

    //count of an ItemID on hand
    public int countItem(int itemID)
    {
        int count = 0;
        foreach(Item item in items)
        {
            if (item == null || item.itemNo != itemID) continue;

            count += item.quantity;

        }

        return count;
    }

}
