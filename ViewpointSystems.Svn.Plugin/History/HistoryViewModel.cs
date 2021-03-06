using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using NationalInstruments.Core;
using NationalInstruments.Shell;

namespace ViewpointSystems.Svn.Plugin.History
{
    public class HistoryViewModel : IToolWindowViewModel// ToolWindowViewModelBase
    {
        //TODO: grab SvnManagerPlugin and call History

        private ToolWindowEditSite _editSite;

        public HistoryViewModel(ToolWindowEditSite site)
        {
            _editSite = site;
            
            //site.RootEditSite.ActiveDocumentChanged += HandleActiveDocumentChanged;
        }

        private string documentName;
        public string DocumentName
        {
            get { return documentName; }
            set
            {
                documentName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Called when the active document changes
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event data</param>
        private void HandleActiveDocumentChanged(object sender, ActiveDocumentChangedEventArgs args)
        {

            if (args.ActiveDocument == null)
            {
                DocumentName = "No Selection";
                //_documentTypeControl.Text = "No Selection";
                return;
            }
            var name = args.ActiveDocument.DocumentName;
            var type = args.ActiveDocument.Envoy.ModelDefinitionType.ToString();

            DocumentName = name;
            //_documentTypeControl.Text = type;
        }



        public object Model
        {
            get { return null; }
        }

        public IViewModel ParentViewModel
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public object TryFindResource(object key)
        {
            return null;
        }

        public PlatformVisual View
        {
            get { return new HistoryView(_editSite, this); }
        }

        public string Name
        {
            get
            {
                return "SVN History";
            }
        }

        public ImageSource SmallImage
        {
            get
            {
                return null;
            }
        }

        public QueryResult<T> QueryService<T>() where T : class
        {
            return new QueryResult<T>();
        }

        public void Initialize(IToolWindowTypeInfo info)
        {
        }



        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}