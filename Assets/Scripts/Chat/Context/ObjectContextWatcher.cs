using TMPro;
using UnityEngine;

public class ObjectContextWatcher : MonoBehaviour
{
    public ObjectType type = ObjectType.Generic;

    public bool trackKinematics = false;

    public new string name = "";
    public TextMeshProUGUI nameText;
    [TextArea(3, 6)]
    public string description = "";

    private ObjectContext _objectContext;
    private SceneContextManager _scm;

    void Awake()
    {
        _objectContext = new ObjectContext
        {
            Name = name,
            Description = description,
            ObjectKinematic = trackKinematics ?
                new ObjectKinematicContext(transform, GetComponent<Rigidbody>()) :
                null
        };

        _scm = FindAnyObjectByType<SceneContextManager>();

        switch (type)
        {
            case ObjectType.Generic:

                break;
            case ObjectType.NPC:
                _scm.SetContext(gameObject, _objectContext);

                break;
        }

        //nameText.text = name;
    }

    // public void SetContext(string[] cont)
    // {
    //     _objectContext = cont;
    //     _scm.SetContext(gameObject, _objectContext);
    // }

    public string GetContext() => _scm.GetContext(gameObject);
}
