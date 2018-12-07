using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectController : MonoBehaviour {

    List<GameObject> objectsInScene = new List<GameObject>();
    public GameObject myBlockInstance;
    public GameObject prevBlock;
    public List<GameObject> cars = new List<GameObject>();
    bool placing;
    bool previous;
    //float maxHeight;
    Stack<float> maxHeight = new Stack<float>();
    public AudioSource music;
    int musicCounter;
    public AudioClip[] song;
    public AudioSource soundEffect, soundEffect2;
    public GameObject instructions, height;
    private Text text;

    public GameObject LongBlock, CubeBlock, LBlock, TallBlock, Tree, Bench;
    GameObject toPlace;

    // Use this for initialization
    void Start () {
        placing = false;
        previous = false;

        toPlace = chooseObject();

        maxHeight.Push(0);

        musicCounter = 0;
        music.clip = song[musicCounter];
        music.Play();
        musicCounter++;
        StartCoroutine(textManager());
    }
	
	// Update is called once per frame
	void Update () {
        manageAudio();
        driveCars();
        if (previous && OVRInput.GetDown(OVRInput.Button.Two)) // Destroy previous block
        {
            Destroy(prevBlock);
            objectsInScene.Remove(prevBlock);
            previous = false;
            maxHeight.Pop();
            if (maxHeight.Count == 0)
                maxHeight.Push(0);
        }
        if (OVRInput.Get(OVRInput.Button.Three)) // Move up
        {
            float ypos = this.transform.position.y;
            ypos += 0.075f;
            Vector3 newPos = new Vector3(this.transform.position.x, ypos, this.transform.position.z);
            this.transform.position = newPos;
        }
        else if (OVRInput.Get(OVRInput.Button.Four)) // Move down
        {
            float ypos = this.transform.position.y;
            ypos -= 0.075f;
            Vector3 newPos = new Vector3(this.transform.position.x, ypos, this.transform.position.z);
            this.transform.position = newPos;
        }
        if (OVRInput.GetDown(OVRInput.Button.One) && OVRInput.GetDown(OVRInput.Button.Two)) // Clear the scene
        {
            for (int i = 0; i < objectsInScene.Count; i++)
                Destroy(objectsInScene[i]);
            previous = false;
            while (maxHeight.Count > 0)
                maxHeight.Pop();
            maxHeight.Push(0);
        }
        else if (placing) // While user is still deciding where to place the object
        {
            myBlockInstance.GetComponent<Rigidbody>().freezeRotation = true;
            myBlockInstance.transform.position = this.transform.position + this.transform.forward * 5f - this.transform.up * 4f;
            if (OVRInput.GetDown(OVRInput.Button.One)) // User places object
            {
                soundEffect.Play();

                myBlockInstance.GetComponent<Rigidbody>().useGravity = true;
                myBlockInstance.GetComponent<Rigidbody>().freezeRotation = false;
                myBlockInstance.GetComponent<Rigidbody>().mass = 2;
                objectsInScene.Add(myBlockInstance);

                prevBlock = myBlockInstance;
                previous = true;
                placing = false;

                toPlace = chooseObject();

                float ypos = this.transform.position.y;
                ypos += prevBlock.GetComponent<BoxCollider>().size.y / 2 + toPlace.GetComponent<BoxCollider>().size.y / 2 + 0.1f;
                Vector3 newPos = new Vector3(this.transform.position.x, ypos, this.transform.position.z);
                this.transform.position = newPos;
            }
            else if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick)) // Rotate Horizontally
                myBlockInstance.transform.Rotate(0, Time.deltaTime * 30f, 0);
            else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick)) // Rotate Vertically
                myBlockInstance.transform.Rotate(0, 0, Time.deltaTime * 30f);
        } 
        else if (!placing && OVRInput.GetDown(OVRInput.Button.One)) {
            print("Input detected");
            placing = true;

            myBlockInstance = Instantiate<GameObject>(toPlace, new Vector3(0,-.65f,0), Quaternion.identity);
            myBlockInstance.GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            if (previous && prevBlock.transform.position.y > maxHeight.Peek() && prevBlock.GetComponent<Rigidbody>().velocity.y == 0.0f)
            {
                maxHeight.Push(prevBlock.transform.position.y);
                soundEffect2.Play();
                StartCoroutine(showHide());
            }
        }
	}

    IEnumerator textManager()
    {
        height.SetActive(false);
        yield return new WaitForSeconds(15);
        instructions.SetActive(false);
    }

    IEnumerator showHide()
    {
        //if (!instructions.activeSelf)
        {
            height.SetActive(true);
            text = height.GetComponent<Text>();
            text.text = maxHeight.Peek().ToString();
            yield return new WaitForSeconds(5);
            height.SetActive(false);
        }
    }

    GameObject chooseObject()
    {
        int choice = Random.Range(1, 9);
        if (choice == 1 || choice == 2)
            return LongBlock;
        else if (choice == 3 || choice == 4)
            return LBlock;
        else if (choice == 5 || choice == 6)
            return CubeBlock;
        else if (choice == 7 || choice == 8)
            return TallBlock;
        else if (choice == 9 || choice == 10)
            return Tree;
        else
            return Bench;
    }

    void driveCars()
    {
        foreach (var car in cars)
        {
            car.transform.position += car.transform.forward * 0.01f;
            if (car.transform.position.z > 40f)
            {
                car.transform.position = new Vector3(-1.02f, -1.14f, -40f);
                car.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            }
            else if (car.transform.position.z < -40f)
            {
                car.transform.position = new Vector3(-0.03f, -1.14f, 40f);
                car.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            }
        }
    }

    void manageAudio()
    {
        if (!music.isPlaying)
        {
            music.clip = song[musicCounter];
            music.Play();
            musicCounter = (musicCounter + 1) % 3;
        }
    }
}
