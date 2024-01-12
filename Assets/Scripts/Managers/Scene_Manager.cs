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
    Quest_Manager qm;
    Input_Manager inputMan;

    entrance currentEntrance;
    public entrance defaultEntrance;

    Image fadeImage;

    string sceneName;

    bool loadingFromDisk = false;


    //consider 30fps
    float fadeInSeconds = 0.9f;
    float fadeOutSeconds = 0.4f;
    int fadeSteps = 20;

    bool fadingOut = false;
    bool fadingIn = false;
    Color fade;

    string[] sortingLayers;

    string targetSceneName;


    //public bool manageLayers;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        sceneName = SceneManager.GetActiveScene().name;


    }


    private void OnEnable()
    {

        qm = this.GetComponent<Quest_Manager>();
        im = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Inventory_Manager>();
        inputMan = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Input_Manager>();

        fadeImage = GameObject.FindGameObjectWithTag("HUD").transform.Find("Fade").GetComponent<Image>();
        pp = this.GetComponent<Resource_Manager>().getPlayerPersistence();


        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        fade = new Color(0, 0, 0, 0);

        if (sp != null)
            sp.initialize();

        sortingLayers = new string[SortingLayer.layers.Length];

        foreach (SortingLayer layer in SortingLayer.layers)
        {
            sortingLayers[SortingLayer.GetLayerValueFromID(layer.id)] = layer.name;
        }

        if (Time.time < 0.1f) //??????? TODO what does this mean
        {
            pp.setChangingScenes(false);
        }

        //Find the Entrance to this scene which was set by previous scene's exit
        currentEntrance = null;

        byte targetEntrance;
        if (pp.fromSave()) targetEntrance = pp.getSaveData().getEntranceNo();
        else
            targetEntrance = pp.getEntranceNo();

        if (pp.fromSave()) loadPersistenceFromSave();


        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Entrance"))
        {
            entrance ent = go.GetComponent<entrance>();
            if (ent.getEntranceNo() == pp.getEntranceNo())
            {
                currentEntrance = ent;

                break;
            }
        }
        if (currentEntrance == null)
        {
            Debug.Log("Error: No Scene Entrance Found");
            currentEntrance = defaultEntrance;
            if (defaultEntrance == null)
                Debug.Log("Error: No Default Entrance Set");
        }


        pp.setChangingScenes(false);

        setFadeIn();

        

    }

    void loadPersistenceFromSave()
    {
        //load all data in the save file into the pp object

    }


    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreLayerCollision(0, 8, true);
        //player.GetComponent<Player>().unfreeze();

        //set character position from SO
        //if none, use a default spawn point

        //find object of name in entrance SO variable



        

    }

    public string getSceneName()
    {
        return this.sceneName;
    }

    public void triggerSceneChange(string targetScene, byte entranceNo)
    {
        storeSceneChange();
        exitScene(targetScene, entranceNo);
    }    

    void exitScene(string targetSceneName, byte entranceNo)
    {
        
        if (pp.isChangingScenes()) return;
        
        player.GetComponent<Player>().freeze();

        pp.setChangingScenes(true);


        //set the next scene entrance
        pp.setEntrance(entranceNo);

        this.targetSceneName = targetSceneName;

        setFadingOut();   

    }

    void storeSceneChange()
    {
        //store health
        pp.setHealth(player.getHealth());

        //Store Inventory Data
        im.storePersistenceData();

        //Store Quest Data
        qm.storePersistenceData();

        if (sp != null)
            sp.exitScene();
    }


    public void loadSaveFile(SaveData save)
    {
        pp.setSaveFile(save);
        exitScene(save.getSceneName(), save.getEntranceNo());
    }



    void setFadingOut()
    {
        fadeImage.enabled = true;
        fadingOut = true;
        fade.a = 0;
        fadeImage.color = fade;
    }

    public entrance getEntrance()
    {
        return this.currentEntrance;
    }



    public int getHealth()
    {
        if (pp.fromSave()) return pp.getSaveData().getHealth();
        return pp.getHealth();
    }

    private void FixedUpdate()
    {
        if(fadingOut)
        {
            fade = fadeImage.color;
            if(fade.a < 1)
            {
                fade.a += 1f/fadeSteps;
                fadeImage.color = fade;
            }
            else
            {
                fade.a = 1;
                fadeImage.color = fade;
                SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
                //End of Fade Out Sequence
            }
        }

        else if(fadingIn)
        {
            fade = fadeImage.color;
            if(fade.a > 0)
            {
                fade.a -= 1f / fadeSteps;
                fadeImage.color = fade;

                if (fade.a < 0.5f)
                {
                    inputMan.enableGameplay();
                    //inputMan.setInputState(InputState.inGame);
                    player.unfreeze();
                }
                    
            }
            else
            {
                fade.a = 0;
                fadeImage.color = fade;
                fadeImage.enabled = false;
                this.fadingIn = false;
                //End of Fade In Sequence
            }
        }
        
    }

    void setFadeIn()
    {
        fadeImage.enabled = true;
        fade.a = 1;
        fadeImage.color = fade;

        this.fadingIn = true;

    }

    public void incrementLayer(GameObject obj, sbyte increment)
    {
        SpriteRenderer[] srs = obj.GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer sr in srs)
        {
            sr.sortingLayerName = sortingLayers[SortingLayer.GetLayerValueFromID(sr.sortingLayerID) + increment*3];
            
        }
    }


    public Player_Persistence getPlayerPersistence() { return this.pp; }
    

    public Scene_Persistence getSP() { return sp; }


    public void getQuestData(ref bool[] completion, ref List<SerializableQuest> squests)
    {

        if (Time.time < 0.5f) return;
        
        this.pp.getQuestData(ref completion, ref squests);
    }

}
