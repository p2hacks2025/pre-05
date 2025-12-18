using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using TMPro;

public class DatabaseManager : MonoBehaviour
{
    // 名前空間をフルで指定して「あいまいさ」を回避
    private Firebase.Database.DatabaseReference mDatabaseRef;
    private string firebaseDatabaseUrl = "https://comparisonapp-bd9e5-default-rtdb.asia-southeast1.firebasedatabase.app/";

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new System.Uri(firebaseDatabaseUrl);
                mDatabaseRef = FirebaseDatabase.GetInstance(app, firebaseDatabaseUrl).RootReference;
                Debug.Log("<color=green>Firebase接続完了</color>");
            }
        });
    }

    public void SavePostToDatabase(string postId, string title, List<string> imageNames, System.Action<string> onComplete)
    {
        if (mDatabaseRef == null) return;
        Dictionary<string, object> postData = new Dictionary<string, object>
        {
            { "title", title },
            { "imageNames", imageNames },
            { "createdAt", Firebase.Database.ServerValue.Timestamp }
        };
        mDatabaseRef.Child("Posts").Child(postId).SetValueAsync(postData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                string shareUrl = "https://comparisonapp-bd9e5.web.app/play?id=" + postId;
                onComplete?.Invoke(shareUrl);
            }
        });
    }

    public void GetPostData(string postId, System.Action<string, List<string>> onComplete)
    {
        if (mDatabaseRef == null) return;
        mDatabaseRef.Child("Posts").Child(postId).GetValueAsync().ContinueWithOnMainThread((System.Threading.Tasks.Task<Firebase.Database.DataSnapshot> task) => {
            if (task.IsCompleted && task.Result.Exists)
            {
                Firebase.Database.DataSnapshot snapshot = task.Result;
                string title = snapshot.Child("title").Value?.ToString() ?? "無題";
                List<string> names = new List<string>();
                foreach (var nameSnap in snapshot.Child("imageNames").Children) names.Add(nameSnap.Value.ToString());
                onComplete?.Invoke(title, names);
            }
        });
    }

    public void GetAllPosts(System.Action<List<Dictionary<string, object>>, List<string>> onComplete)
    {
        if (mDatabaseRef == null) return;
        mDatabaseRef.Child("Posts").GetValueAsync().ContinueWithOnMainThread((System.Threading.Tasks.Task<Firebase.Database.DataSnapshot> task) => {
            if (task.IsCompleted && task.Result.Exists)
            {
                List<Dictionary<string, object>> allPosts = new List<Dictionary<string, object>>();
                List<string> postIds = new List<string>();
                foreach (var postSnap in task.Result.Children)
                {
                    var data = postSnap.Value as Dictionary<string, object>;
                    if (data != null)
                    {
                        allPosts.Add(data);
                        postIds.Add(postSnap.Key);
                    }
                }
                onComplete?.Invoke(allPosts, postIds);
            }
        });
    }
}