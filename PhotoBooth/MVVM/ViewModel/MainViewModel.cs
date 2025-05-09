using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Mvvm;
using Emgu.CV;
using Emgu.CV.Structure;
using ViewBaseModel = PhotoBooth.Abstract.ViewModelBase;

namespace PhotoBooth.MVVM.ViewModel
{
    class MainViewModel : ViewBaseModel
    {

        public DelegateCommand TakePhotoCommand { get; set; }
        public DelegateCommand StartCameraCommand { get; set; }
        public DelegateCommand<Canvas> SaveCanvasCommand { get; set; }
        public DelegateCommand RetakeCapturedCommand { get; set; }
        public DelegateCommand ResetCapturedCommand { get; set; }


        #region Properties
        private VideoCapture _liveImage;

        public VideoCapture LiveImage
        {
            get { return _liveImage; }
            set { _liveImage = value; RaisePropertiesChanged(nameof(LiveImage)); }
        }

        private DispatcherTimer _timer;

        public DispatcherTimer Timer
        {
            get { return _timer; }
            set { _timer = value; RaisePropertiesChanged(nameof(Timer)); }
        }

        private BitmapSource _cameraImage;

        public BitmapSource CameraImage
        {
            get { return _cameraImage; }
            set { _cameraImage = value; RaisePropertiesChanged(nameof(CameraImage)); }
        }

        private BitmapSource _capturedImage1;
        public BitmapSource CapturedImage1
        {
            get => _capturedImage1;
            set { _capturedImage1 = value; RaisePropertiesChanged(nameof(CapturedImage1)); }
        }

        private BitmapSource _capturedImage2;
        public BitmapSource CapturedImage2
        {
            get => _capturedImage2;
            set { _capturedImage2 = value; RaisePropertiesChanged(nameof(CapturedImage2)); }
        }

        private BitmapSource _capturedImage3;
        public BitmapSource CapturedImage3
        {
            get => _capturedImage3;
            set { _capturedImage3 = value; RaisePropertiesChanged(nameof(CapturedImage3)); }
        }

        private BitmapSource _capturedImage4;
        public BitmapSource CapturedImage4
        {
            get => _capturedImage4;
            set { _capturedImage4 = value; RaisePropertiesChanged(nameof(CapturedImage4)); }
        }

        private bool _noCameraFound;

        public bool NoCameraFound
        {
            get { return _noCameraFound; }
            set { _noCameraFound = value; RaisePropertiesChanged(nameof(NoCameraFound)); }
        }

        private bool _cameraFound;

        public bool CameraFound
        {
            get { return _cameraFound; }
            set { _cameraFound = value; RaisePropertiesChanged(nameof(CameraFound)); }
        }

        private string _titleName;

        public string TitleName
        {
            get { return _titleName; }
            set { _titleName = value; RaisePropertiesChanged(nameof(TitleName)); }
        }


        #endregion


        public MainViewModel()
        {

            Assembly assembly = Assembly.GetExecutingAssembly();
            ApplicationName = assembly.GetName().Name;
            ApplicationVersion = assembly.GetName().Version;

            TakePhotoCommand = new DelegateCommand(TakePhoto);
            StartCameraCommand = new DelegateCommand(StartCamera);
            RetakeCapturedCommand = new DelegateCommand(RetakeCaptured);
            ResetCapturedCommand = new DelegateCommand(ResetCaptured);
            SaveCanvasCommand = new DelegateCommand<Canvas>(SaveCanvas);
            NoCameraFound = false;
            CameraFound = true;
        }

        

        #region Fields
        private int _captureCount = 0;
        private int CurrentCaptureIndex;

        #endregion

       


       


        //Live Camera initialization
        private void StartCamera()
        {
            try
            {
                if (LiveImage == null)
                {
                    LiveImage = new VideoCapture(0);
                    CameraFound = false;
                    NoCameraFound = true;
                }

                if (LiveImage.IsOpened)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromMilliseconds(30);
                    _timer.Tick += (s, e) => ProcessFrame();
                    _timer.Start();
                    NoCameraFound = false;
                    CameraFound = true;
                }
                else
                {
                    //WarningMessage("Camera could not be opened.");
                    CameraFound = false;
                    NoCameraFound = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }



        /// <summary>
        /// process the LiveImage frame from camera
        /// </summary>
        private void ProcessFrame()
        {
            try
            {

                if (LiveImage != null && LiveImage.Ptr != IntPtr.Zero)
                {

                    LiveImage.Grab();
                    // Retrieve the current frame from the camera
                    Mat frame = new Mat();
                    LiveImage.Retrieve(frame);

                    if (!frame.IsEmpty)
                    {
                        // Convert the frame to BitmapImage and display
                        CameraImage = ConvertMatToBitmapImage(frame);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        private void TakePhoto()
        {
            try
            {

                if (LiveImage != null && LiveImage.Ptr != IntPtr.Zero)
                {
                    // Capture the current frame
                    Mat frame = new Mat();
                    LiveImage.Retrieve(frame);

                    if (!frame.IsEmpty)
                    {
                        BitmapSource photo = ConvertMatToBitmapImage(frame);

                        // Set into the next available CapturedImage slot
                        _captureCount++;

                        switch (_captureCount)
                        {
                            case 1:
                                CapturedImage1 = photo;
                                break;
                            case 2:
                                CapturedImage2 = photo;
                                break;
                            case 3:
                                CapturedImage3 = photo;
                                break;
                            case 4:
                                CapturedImage4 = photo;
                                break;
                            default:
                                // Already 4 captured -> reset
                                _captureCount = 4;
                                CapturedImage4 = photo;
                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void RetakeCaptured()
        {
            try
            {
                if (LiveImage != null && LiveImage.Ptr != IntPtr.Zero)
                {
                    // Capture the current frame
                    Mat frame = new Mat();
                    LiveImage.Retrieve(frame);

                    if (!frame.IsEmpty)
                    {
                        BitmapSource photo = ConvertMatToBitmapImage(frame);

                        CurrentCaptureIndex = _captureCount - 1;

                        switch (CurrentCaptureIndex)
                        {
                            case 0:
                                CapturedImage1 = photo;
                                break;
                            case 1:
                                CapturedImage2 = photo;
                                break;
                            case 2:
                                CapturedImage3 = photo;
                                break;
                            case 3:
                                CapturedImage4 = photo;
                                break;
                            default:
                                // Already 4 captured -> reset
                                _captureCount = 4;
                                CapturedImage4 = photo;
                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private BitmapImage ConvertMatToBitmapImage(Mat frame)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Convert Mat to byte array and save to memory stream as JPEG
                    byte[] imageBytes = frame.ToImage<Bgr, byte>().ToJpegData(); // Convert to JPEG byte array
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    ms.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Freeze to make it cross-thread accessible
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        //Save Canvas to file
        private void SaveCanvas(Canvas canvas)
        {
            if (canvas == null)
                return;

            // Render Canvas to bitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight,96d, 96d, PixelFormats.Pbgra32);

            renderBitmap.Render(canvas);

            string filePath = ShowSaveDialog(TitleName);

            if (!string.IsNullOrEmpty(filePath))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fileStream);
                }
            }

        }

        private void ResetCaptured()
        {
            try
            {
                CapturedImage1 = null;
                CapturedImage2 = null;
                CapturedImage3 = null;
                CapturedImage4 = null;
                _captureCount = 0;
                CurrentCaptureIndex = 0;
                TitleName = string.Empty;

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }
    }

}
