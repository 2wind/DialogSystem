using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


namespace DialogSystem
{
    /* 필요한 기능 목록: 
      * 스탠딩 이미지를 spawn해서, 리스트...가 아니라 딕셔너리에 들고 있는다. 
      클래스와 같은 느낌으로 사용할 수 있도록 한다.
      * 스탠딩 이미지는 id를 가지고 spawn되는데, 이 
      id를 이용해 텍스트에서 FI/FO, 이동, Wiggle 등의 F/X,
      대화에서의 밝아지고 어두워짐을 제어할 수 있다. 
      ("spawn:스탠딩 경로:할당할 id" "id:_fadein:" "id:_move:right")
      * 텍스트의 경우 "id:제목:본문"의 간편한 방식을 사용한다. 
       한 줄씩 읽어오는 인터프리터라고 할 수 있다. 
      * background 제어를 위해서는 id == background로.
       ("background:_fadein:" "background:_change:")
      * 리스트에 Character 클래스 방식으로 저장되어, 순서를 바꾸고 제어할 수 있다.
      * 구현해야 하는 기능:
       1. 소리 지원
       2. 여러 개의 (서로 다르거나 같은) 액션이 한 번에 일어날 수 있도록 수정
       3. 스프라이트 교체(웃는 얼굴, 화난 스탠딩 등)
     */


    public class Script
    {
        string _id;
        string _title;
        string _mainText;
        Script nextAction = null;
        
        public Script(string id, string title, string mainText)
        {
            _id = id;
            _title = title;
            _mainText = mainText;
        }
        public string GetId() { return _id; }
        public string GetTitle() { return _title; }
        public string GetMainText() { return _mainText; }
        public bool HasNextAction() { if (nextAction != null) { return true; } return false; }
        public Script GetNextAction() { return nextAction; }
        public void SetNextAction(Script action)
        {
            nextAction = action;
        }


    }

    public class Character
    {
        string _id;
        GameObject _chObject;
        public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public Character(string id, GameObject chObject)
        {
            _id = id;
            _chObject = chObject;
        }
        
        public string GetId() { return _id; }
        public GameObject GetObject() { return _chObject; }
        public void SetSprite(string emotion)
        {
            _chObject.GetComponent<Image>().overrideSprite = sprites[emotion];

        }
        
    }

    

    public class DialogManager : MonoBehaviour
    {
        public Image backgroundImage;
        public GameObject characterSpawnPosition;
        public Image foregroundImage;
        public Text title;
        public Text mainText;

        // 화면에 보이는 부분을 제어하는 곳

        //public Dictionary<string, GameObject> characters = new Dictionary<string, GameObject>();
        public List<Character> charactersN = new List<Character>();
        public List<Script> initialScripts = new List<Script>();
        public List<Script> scripts = new List<Script>();


        // 캐릭터와 스크립트를 담고 있는 리스트.

        public static DialogManager instance;
        bool isActive = false;

        void Start()
        {
            instance = this;
            gameObject.GetComponent<CanvasGroup>().alpha = 0;
            gameObject.GetComponent<CanvasGroup>().interactable = false;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            //  Initialize("sample_dialog");
        }

        void Update() 
        {
            if (isActive && Input.GetButtonDown("Fire1"))
            {
                if (!scripts.Any())
                {
                    //여기서 종료 처리를 해준다.
                    Debug.Log("시연이 끝났습니다.");
                    DisableDialog();
                    return;
                }
                else
                {
                    Script topLine = scripts[0];

                    scripts.RemoveAt(0);//POP
                    InterpretLine(topLine);
                    if (topLine.HasNextAction()) { InterpretLine(topLine.GetNextAction()); }
                }

            }
            
        }

        public void Reset()
        {
            if (charactersN.Any())
            {
                charactersN.Select<Character,GameObject>(obj => obj.GetObject()).ToList().ForEach(Destroy);
            }
            charactersN.Clear();
            title.text = "";
            mainText.text = "";//스크립트도 날리고
            scripts.Clear(); //리스트를 날리고 
            initialScripts.Clear();
            backgroundImage.sprite = null;
            //TODO: 이미지도 기본으로 날려야 한다
        }

        public void Initialize(string scriptName)
        {
            Reset();
            gameObject.GetComponent<CanvasGroup>().alpha = 1;
            gameObject.GetComponent<CanvasGroup>().interactable = true;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
            scripts = ScriptFileToList(scriptName);
            foreach(Script script in initialScripts)
            {
                InterpretLine(script);
            }
            isActive = true;

        }

        void DisableDialog()
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 0;
            gameObject.GetComponent<CanvasGroup>().interactable = false;
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            isActive = false;
        }



        public List<Script> ScriptFileToList(string scriptName)
        {
            List<Script> scripts = new List<Script>();
            try
            {
                TextAsset rawScript = Resources.Load(scriptName) as TextAsset;
                List<string> rawScriptList = rawScript.text.Split('\n').ToList();
                foreach (string line in rawScriptList)
                {
                    if (line.StartsWith("initial"))//initial:시작 시 일어날 동작들
                    {
                        string[] scriptLine = line.Split(':');
                        Debug.Log(scriptLine.Length);
                        Script _script = new Script(scriptLine[1], scriptLine[2], scriptLine[3]);
                        if (scriptLine.Length == 7)
                        {
                            Script nextScript = new Script(scriptLine[4], scriptLine[5], scriptLine[6]);
                            _script.SetNextAction(nextScript);
                        }
                        initialScripts.Add(_script);
                    }
                    else
                    {
                        string[] scriptLine = line.Split(':');
                        Debug.Log(scriptLine.Length);
                        Script _script = new Script(scriptLine[0], scriptLine[1], scriptLine[2]);
                        if (scriptLine.Length == 6)
                        {
                            Script nextScript = new Script(scriptLine[3], scriptLine[4], scriptLine[5]);
                            _script.SetNextAction(nextScript);
                        }
                        scripts.Add(_script);
                    }
                }
            }
            catch (System.Exception)
            {
                Debug.Log("script not found");
                throw;
            }
            return scripts;
        }

        public void InterpretLine(Script topLine)
        {
            
            {                
                if (topLine.GetId() == "background")
                {
                    switch (topLine.GetTitle())
                    {
                        case "change":

                            backgroundImage.sprite = Resources.Load<Sprite>(topLine.GetMainText().Trim());
                            break;
                        case "clear":
                            backgroundImage.sprite = null;
                            break;

                        default:
                            break;
                    }
                    return;

                }//배경 조절. background:change:경로 background:clear:
                else if (topLine.GetId().Contains("spawn"))
                {
                    SpawnCharacter(topLine);
                }
                else if (topLine.GetId() == "sound")
                {
                    
                    gameObject.GetComponent<AudioSource>().clip = Resources.Load(topLine.GetTitle()) as AudioClip;
                    gameObject.GetComponent<AudioSource>().Play();
                }//sound:경로명: or (sound:경로명:bgm 이쪽은 아직 미구현..)
                else
                {
                    if (topLine.GetTitle().StartsWith("_"))
                    {
                        ControlCharacter(topLine);

                    }//id:컨트롤 방법(_로 시작):
                    else
                    {
                        //id가 같으면 white, 다르면 lightgray.
                        foreach (Character character in charactersN)
                        {
                            if (character.GetId() == topLine.GetId())
                            {
                                LeanTween.color(character.GetObject().GetComponent<RectTransform>(), Color.white, 0.2f);
                            }
                            else
                            {
                                LeanTween.color(character.GetObject().GetComponent<RectTransform>(), new Color(0.8f, 0.8f, 0.8f, 1), 0.2f);
                            }
                        }
                        title.text = topLine.GetTitle();
                        mainText.text = topLine.GetMainText();

                    }//id:제목:대사 
                }
            }

        }//파이선에서의 readline과 같이 작동하도록 설계.

        void SpawnCharacter(Script topLine) //spawn_replace_left
        {

            {
                bool spawnToLeft = false;
                bool isReplace = false;
                bool isSlide = false;
                GameObject newCharacter = Instantiate(Resources.Load(topLine.GetTitle()) as GameObject, new Vector3(640, 0, 0), Quaternion.identity) as GameObject;
                newCharacter.transform.SetParent(characterSpawnPosition.transform, false);
                //move from left, right, top, bottom
                //spawn_from_left....
                if (topLine.GetId().Contains("to_left"))
                {
                    spawnToLeft = true;
                }
                if (topLine.GetId().Contains("replace"))
                {
                    isReplace = true;
                }

                if (topLine.GetId().Contains("from_left"))
                {
                    newCharacter.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, 0);
                    isSlide = true;
                }
                else if (topLine.GetId().Contains("from_right"))
                {
                    newCharacter.GetComponent<RectTransform>().anchoredPosition = new Vector3(1500, 0);
                    isSlide = true;
                }
                else if (topLine.GetId().Contains("from_top"))
                {
                    newCharacter.GetComponent<RectTransform>().anchoredPosition = new Vector3(640, 700);
                    isSlide = true;
                }
                else if (topLine.GetId().Contains("from_bottom"))
                {
                    newCharacter.GetComponent<RectTransform>().anchoredPosition = new Vector3(640, -750);
                    isSlide = true;
                }
                if (!isSlide)
                {
                    newCharacter.GetComponent<CanvasRenderer>().SetAlpha(0);
                    newCharacter.GetComponent<Image>().CrossFadeAlpha(1.0f, 0.5f, false);
                }
                //newCharacter.GetComponent<RectTransform>().transform;
                if (isReplace)
                {
                    if (spawnToLeft)
                    {
                        Destroy(charactersN[0].GetObject());
                        charactersN[0] = new Character(topLine.GetMainText().Trim(), newCharacter);
                    }
                    else
                    {
                        var lastObj = charactersN.Last();
                        Destroy(lastObj.GetObject());
                        lastObj = new Character(topLine.GetMainText().Trim(), newCharacter);
                    }
                }
                else
                {
                    if (spawnToLeft)
                    {
                        charactersN.Insert(0, new Character(topLine.GetMainText().Trim(), newCharacter));
                    }
                    else
                    {
                        charactersN.Add(new Character(topLine.GetMainText().Trim(), newCharacter));
                    }
                    SortCharactersPosition();
                }

                //  characters.Add(topLine.GetMainText().Trim(), newCharacter); //좌측에서 spawn되는 경우 맨 첫번째에 spawn해야 한다.

            }
        }
       

        void ControlCharacter(Script line)
        {
            

            if (charactersN.Any(obj => obj.GetId() ==line.GetId()))
            {
                //Debug.Log(charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<Image>().color);
                switch (line.GetTitle())
                {
                    case "_fadein": // 이런 느낌으로 제어문 추가
                                    // characters[line.GetId()].GetComponent<Image>().CrossFadeColor(Color.white, 0.5f, false, true);
                        LeanTween.color(charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<RectTransform>(), Color.white, 0.5f);
                        //Debug.Log(charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<Image>().color);
                        //charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<Image>().CrossFadeColor(Color.white, 0.5f, false, true);
                        //characters[line.GetId()].GetComponent<Image>().CrossFadeAlpha(1.0f, 0.5f, false);

                        break;
                    case "_fadeout":
                        //  characters[line.GetId()].GetComponent<Image>().CrossFadeColor(Color.gray, 0.5f, false, true);
                        LeanTween.color(charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<RectTransform>(), Color.gray, 0.5f);
                        //charactersN.Find(obj => obj.GetId() == line.GetId()).GetObject().GetComponent<Image>().CrossFadeColor(Color.white, 0.5f, false, true);

                        break;
                    case "_wiggle":


                        break;
                    case "_remove":
                        StartCoroutine(RemoveCharacter(line.GetId()));
                       
                        break;
                    case "_change":
                        ChangeSprite(line.GetId(), line.GetMainText());
                        break;


                    default:
                        break;
                }//id:컨트롤 방법(_로 시작):
            }

        }

        void SortCharactersPosition()
        { ////1280x720을 기준 해상도로 잡아, 적절하게 캐릭터들을 배치해주고 그 과정에서
            //캐릭터를 자연스럽게 움직여준다.
            /*
            for (int i = 0; i < characters.Count; i++)
            {
                float spacing = ((1280 - (300 * characters.Count)) / (characters.Count + 1));
                LeanTween.move(characters.Values.ToList()[i].GetComponent<RectTransform>(),
                    new Vector3(((2* i) + 1)* 150 + (i + 1) * spacing, 0), 0.3f);
            }
            */
            for (int i = 0; i < charactersN.Count; i++)
            {
                float spacing = ((1280 - (300 * charactersN.Count)) / (charactersN.Count + 1));
                LeanTween.move(charactersN[i].GetObject().GetComponent<RectTransform>(),
                    new Vector3(((2 * i) + 1) * 150 + (i + 1) * spacing, 0), 0.3f);
            }

        }

        IEnumerator RemoveCharacter(string id)
        {
            //   characters[id].GetComponent<Image>().CrossFadeAlpha(0f, 0.5f, false);
            LeanTween.alpha(charactersN.Find(obj => obj.GetId() == id).GetObject().GetComponent<RectTransform>(), 0f, 0.5f);

            //charactersN.Find(obj => obj.GetId() == id).GetObject().GetComponent<Image>().CrossFadeColor(Color.white, 0.5f, false, true);
            yield return new WaitForSeconds(0.5f);
            //   Destroy(characters[id]);
            
            Destroy(charactersN.Find(obj => obj.GetId() == id).GetObject());
            //Destroy(SearchGameObject(charactersN, id));
            //  characters.Remove(id);
            charactersN.RemoveAll(obj => obj.GetId() == id);
            //charactersN.Where(chr => chr.GetId() == id).Select(chr => charactersN.Remove(chr));
            SortCharactersPosition();

        }

        void ChangeSprite(string id, string emotion)
        {
            charactersN.Find(obj => obj.GetId() == id).SetSprite(emotion);
        }



    }
}


