using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace holoutils
{
    /// <summary>
    /// Component that Logs data to a CSV.
    /// Assumes header is fixed.
    /// Copy and paste this logger to create your own CSV logger.
    /// CSV Logger breaks data up into settions (starts when application starts) which are folders
    /// and instances which are files
    /// A session starts when the application starts, it ends when the session ends.
    /// 
    /// In Editor, writes to MyDocuments/SessionFolderRoot folder
    /// On Device, saves data in the Pictures/SessionFolderRoot
    /// 
    /// How to use:
    /// Find the csvlogger
    /// if it has not started a CSV, create one.
    /// every frame, log stuff
    /// Flush data regularly
    /// 
    /// **Important: Requires the PicturesLibrary capability!**
    /// </summary>
    public class CSVLogger : MonoBehaviour

    {
        #region Constants to modify
        private float nextActionTime = 0.0f;
        public float period = 0.1f;
        private const string DataSuffix = "data";
        private const string CSVHeader = "Timestamp,MainObject,Camera.x,Camera.y,Camera.z,Frwd.x,Frwd.y,Frwd.z," +
                                          "Rot.x,Rot.y,Rot.z,Gaze.x,Gaze.y,Gaze.y,Gazedir.x,Gazedir.y,Gazedir.y";
        private const string SessionFolderRoot = "CSVLogger";
        #endregion

        #region private members
        private string m_sessionPath;
        private string m_filePath;
        private string m_recordingId;
        private string m_sessionId;

        private StringBuilder m_csvData;
        #endregion
        #region public members
        public string RecordingInstance => m_recordingId;
        #endregion

        // Use this for initialization
        async void Start()
        {
            await MakeNewSession();
            StartNewCSV();

        }
        async void Update()
        {

            Debug.Log("ff");
            if (Time.time > nextActionTime)
            {
                nextActionTime += period;
                List<String> row = new List<string>();
                Vector3 camera = Camera.main.transform.position;
                Quaternion rot = Camera.main.transform.rotation;
                Vector3 frwd = Camera.main.transform.forward;
                Vector3 gaze = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                Vector3 gazedir = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
                DateTime now = DateTime.Now;
                String name = "Null";
                String date = now.ToString();


                    row.Add(date);
                    row.Add(name);
                    String camx = camera.x.ToString();
                    String camy = camera.y.ToString();
                    String camz = camera.z.ToString();
                    String frwdx = frwd.x.ToString();
                    String frwdy = frwd.y.ToString();
                    String frwdz = frwd.z.ToString();
                    String gazex = gaze.x.ToString();
                    String gazey = gaze.y.ToString();
                    String gazez = gaze.z.ToString();
                    String gazedirx = gazedir.x.ToString();
                    String gazediry = gazedir.y.ToString();
                    String gazedirz = gazedir.z.ToString();
                    String rotx = rot.x.ToString();
                    String roty = rot.y.ToString();
                    String rotz = rot.z.ToString();
                    row.Add(camx);
                    row.Add(camy);
                    row.Add(camz);
                    row.Add(frwdx);
                    row.Add(frwdy);
                    row.Add(frwdz);
                    row.Add(rotx);
                    row.Add(roty);
                    row.Add(rotz);
                    row.Add(gazex);
                    row.Add(gazey);
                    row.Add(gazez);
                    row.Add(gazedirx);
                    row.Add(gazediry);
                    row.Add(gazedirz);
                    AddRow(row);
                    FlushData();
                    //EndCSV();
                }
            


        }
        async Task MakeNewSession()
        {
            m_sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string rootPath = "";
#if WINDOWS_UWP
            StorageFolder sessionParentFolder = Windows.Storage.ApplicationData.Current.LocalFolder;;
            rootPath = sessionParentFolder.Path;
#else
            rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SessionFolderRoot);
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
#endif
            m_sessionPath = Path.Combine(rootPath, m_sessionId);
            Directory.CreateDirectory(m_sessionPath);
            Debug.Log("CSVLogger logging data to " + m_sessionPath);
        }

        public void StartNewCSV()
        {
            m_recordingId = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            var filename = m_recordingId + "-" + DataSuffix + ".csv";
            m_filePath = Path.Combine(m_sessionPath, filename);
            if (m_csvData != null)
            {
                EndCSV();
            }
            m_csvData = new StringBuilder();
            m_csvData.AppendLine(CSVHeader);
        }


        public void EndCSV()
        {
            if (m_csvData == null)
            {
                return;
            }
            using (var csvWriter = new StreamWriter(m_filePath, true))
            {
                csvWriter.Write(m_csvData.ToString());
            }
            m_recordingId = null;
            m_csvData = null;
        }

        public void OnDestroy()
        {
            EndCSV();
        }

        public void AddRow(List<String> rowData)
        {
            AddRow(string.Join(",", rowData.ToArray()));
        }

        public void AddRow(string row)
        {
            m_csvData.AppendLine(row);
        }

        /// <summary>
        /// Writes all current data to current file
        /// </summary>
        public void FlushData()
        {
            using (var csvWriter = new StreamWriter(m_filePath, true))
            {
                csvWriter.Write(m_csvData.ToString());
            }
            m_csvData.Clear();
        }

        /// <summary>
        /// Returns a row populated with common start data like
        /// recording id, session id, timestamp
        /// </summary>
        /// <returns></returns>
        public List<String> RowWithStartData()
        {
            List<String> rowData = new List<String>();
            rowData.Add(Time.timeSinceLevelLoad.ToString("##.000"));
            rowData.Add(m_recordingId);
            rowData.Add(m_recordingId);
            return rowData;
        }

    }
}
