using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

public class Scene_Manager : MonoBehaviour
{
    Player_Persistence pp;

    [SerializeField]
    Scene_Persistence sp;

    Player player;
    Inventory_Manager im;

    entrance currentEntrance;

    Image fadeImage;

    float fadeInSeconds = 0.2f;
    float fadeOutSeconds = 0.2f;
    int fadeSteps = 12;

    bool fadingOut = false;

    string[] sortingLayers;

    //public bool manageLayers;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        pp = Resources.Load<Player_Persistence>("Player Persistence");

        im = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Inventory_Manager>();

        fadeImage = GameObject.FindGameObjectWithTag("HUD").transform.Find("Fade").GetComponent<Image>();

        sortingLayers = new string[SortingLayer.layers.Length];

        foreach(SortingLayer layer in SortingLayer.layers)
        {
            sortingLayers[SortingLayer.GetLayerValueFromID(layer.id)] = layer.name;
        }


        //player.GetComponent<Player>().unfreeze();

        //set character position from SO
        //if none, use a default spawn point

        //find object of name in entrance SO variable

        if(Time.time < 0.1f)
        {
            pp.setChangingScenes(false);
        }


        if (pp.isChangingScenes())
            enterScene();



        fadeIn();

    }

    void enterScene()
    {
        //player.GetComponent<Player>().freeze();

        //Find the Entrance to this scene which was set by previous scene's exit
        currentEntrance = null;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Entrance"))
        {
            entrance e = go.GetComponent<entrance>();
            if (e.getEntranceNo() == pp.getEntranceNo())
            {
                currentEntrance = e;
            }
        }
        if (currentEntrance == null) Debug.Log("Error: No Scene Entrance Found");

        setPlayer();

        pp.setChangingScenes(false);

        //player.GetComponent<Player>().unfreeze();


    }

    public void exitScene(string targetSceneName, byte entranceNo)
    {        
        if (pp.isChangingScenes()) return;
        
        player.GetComponent<Player>().freeze();

        pp.setChangingScenes(true);

        //store health
        pp.setHealth(player.getHealth());
        

        //Item[] tempItems = im.getItems();
        //string[] itemNames = new string[tempItems.Length];

        //for(int i = 0; i<tempItems.Length; i++)
        //{
            //itemNames[i] = tempItems[i].getName();
        //}

        pp.setItems(im.getItems());


        //set the next scene entrance
        pp.setEntrance(entranceNo);


        fadeImage.enabled = true;
        fadingOut = true;
        StartCoroutine(FadeOut(targetSceneName));

    }

    void setPlayer()
    {
        player.sceneInitialize(pp.getHealth(), currentEntrance.transform.position, currentEntrance.getURDL());
    }


    public int getHealth()
    {
        return pp.getHealth();
    }

    IEnumerator FadeOut(string sceneName)
    {   

        while (fadeImage.color.a < 1)
        {
            fadeImage.color = new Color(0, 0, 0, fadeImage.color.a + (1f / fadeSteps));
            yield return new WaitForSeconds(fadeOutSeconds / fadeSteps);
            //yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }


    IEnumerator FadeIn()
    {
        //Fade screen back in



        //wait for Fade to Black to Complete
        //If we decide to change scenes (fade out) while still fading in, cancel the fade in
        while (fadeImage.color.a > 0 && !fadingOut)
        {
            // This allows player to respond to input after the Fade is almost complete, 
            // Better responsiveness of input
            if (fadeImage.color.a < 0.75f) player.unfreeze();

            fadeImage.color = new Color(0, 0, 0, fadeImage.color.a - (1f / fadeSteps));
            yield return new WaitForSeconds(fadeInSeconds / fadeSteps);
            //yield return null;
        }
        if (fadingOut) yield break;


        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.enabled = false;

        player.unfreeze();
        
    }

    void fadeIn()
    {
        fadeImage.enabled = true;
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());

    }

    public void incrementLayer(GameObject obj, sbyte increment)
    {
        SpriteRenderer[] srs = obj.GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer sr in srs)
        {
            sr.sortingLayerName = sortingLayers[SortingLayer.GetLayerValueFromID(sr.sortingLayerID) + increment*3];
            
        }
    }


    public Item_Data[] getItemData()
    {
        return pp.getItemData();
    }

    public int[] getItemCounts()
    {
        return pp.getItemCounts();
    }

    public Scene_Persistence getSP() { return sp; }

}
