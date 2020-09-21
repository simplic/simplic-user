﻿using CommonServiceLocator;
using Simplic.Group;
using Simplic.TenantSystem;
using Simplic.UI.MVC;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Simplic.User.UI
{
    public class UserViewModel : ViewModelBase, IDraggableUser
    {
        #region fields
        private User _user;
        private int _useId;
        private string _userName;
        private string _password;
        private string _firstName;
        private string _lastName;
        private string _email;
        private bool _isActive;
        private bool _isADUser;
        private string _phone;
        private ObservableCollection<GroupViewModel> _groups;
        private ObservableCollection<OrganizationViewModel> _organizations;
        private bool _keepLoggedIn;
        private string _apiKey;
        private int _languageID;
        private readonly IUserService _userService;
        private ICommand _removeGroupCommand;
        private ICommand _removeOrganizationCommand;
        #endregion

        #region ctr
        public UserViewModel()
        {
            _userService = ServiceLocator.Current.GetInstance<IUserService>();
            Groups = new ObservableCollection<GroupViewModel>();
            Organizations = new ObservableCollection<OrganizationViewModel>();
            RemoveGroupCommand = new RelayCommand(OnRemoveGroup);
            RemoveOrganizationCommand = new RelayCommand(OnRemoveOrganization);
        }

        public UserViewModel(User user) : this()
        {
            Groups = new ObservableCollection<GroupViewModel>();
            UserId = user.Ident;
            UserName = user.UserName;
            Password = user.Password;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.EMail;
            IsActive = user.IsActive;
            IsADUser = user.IsADUser;
            KeepLoggedIn = user.KeepLoggedIn;
            ApiKey = user.ApiKey;
            LanguageID = user.LanguageID;
            User = user;
        }
        #endregion

        #region methods
        private void OnRemoveOrganization(object arg)
        {
            if(arg is OrganizationViewModel org)
            {
                Organizations.Remove(org);
                org.Users.Remove(this);
            }
        }

        private void OnRemoveGroup(object arg)
        {
            if(arg is GroupViewModel group)
            {
                Groups.Remove(group);
                group.Users.Remove(this);
            }
        }

        public void Join(ViewModelBase vm)
        {
            if (vm is GroupViewModel groupVm)
            {
                if (groupVm.Users.FirstOrDefault(u => u.UserId == UserId) != null)
                    return;
                groupVm.Users.Add(this);
                Groups.Add(groupVm);
                RaisePropertyChanged("Groups");
            }
            else if(vm is OrganizationViewModel orgVm)
            {
                if (orgVm.Users.FirstOrDefault(u => u.UserId == UserId) != null)
                    return;
                orgVm.Users.Add(this);
                Organizations.Add(orgVm);
                RaisePropertyChanged("Organizations");
            }
        }

        public void SaveUser()
        {
            if (User == null)
            {
                var user = new User
                {
                    Ident = UserId,
                    ApiKey = ApiKey,
                    EMail = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    IsActive = IsActive,
                    IsADUser = IsADUser,
                    KeepLoggedIn = KeepLoggedIn,
                    LanguageID = LanguageID,
                    Password = Password,
                    UserName = UserName
                };
                if (_userService.Register(user))
                    User = user;
            }
            else
                _userService.Save(User);
        }

        public void SavePassword()
        {
            if(_user != null)
                _userService.SetPassword(UserId, Password);
        }

        public void SaveAll()
        {
            SaveUserGroups();
            SaveUserTenants();
        }

        private void SaveUserGroups()
        {
            if(_user != null)
            {
                var groupService = ServiceLocator.Current.GetInstance<IGroupService>();
                var gs = groupService.GetAllByUserId(UserId)?.ToList();
                if(gs != null)
                {
                    var removedGroups = gs.Select(g => g.GroupId).Where(g => !Groups.Select(ng => ng.GroupId).Contains(g));
                    foreach (var removeGroupId in removedGroups)
                        _userService.RemoveGroup(UserId, removeGroupId);
                    var addedGroups = Groups.Select(g => g.GroupId).Where(g => !gs.Select(ng => ng.GroupId).Contains(g));
                    foreach (var addedGroupId in addedGroups)
                        _userService.SetGroup(UserId, addedGroupId);
                }
            }
            else
            {
                foreach (var addedGroup in Groups)
                    _userService.SetGroup(UserId, addedGroup.GroupId);
            }
        }

        private void SaveUserTenants()
        {
            if (_user != null)
            {
                var orgService = ServiceLocator.Current.GetInstance<IOrganizationService>();
                var orgs = orgService.GetByUserId(UserId)?.ToList();
                if (orgs != null)
                {
                    var removedOrganizations = orgs.Select(o => o.Id).Where(o => !Organizations.Select(no => no.OrganizationId).Contains(o));
                    foreach (var removeOrganizationId in removedOrganizations)
                        _userService.RemoveTenant(UserId, removeOrganizationId);
                    var addedOrganizations = Organizations.Select(o => o.OrganizationId).Where(o => !orgs.Select(on => on.Id).Contains(o));
                    foreach (var addedOrganizationId in addedOrganizations)
                        _userService.SetTenant(UserId, addedOrganizationId);
                }
            }
            else
            {
                foreach (var addedGroup in Groups)
                    _userService.SetGroup(UserId, addedGroup.GroupId);
            }
        }
        #endregion

        #region properties
        public ICommand RemoveOrganizationCommand
        {
            get { return _removeOrganizationCommand; }
            set { PropertySetter(value, newValue => _removeOrganizationCommand = newValue); }
        }

        public ICommand RemoveGroupCommand
        {
            get { return _removeGroupCommand; }
            set { PropertySetter(value, newValue => _removeGroupCommand = newValue); }
        }

        public int UserId
        {
            get { return _useId; }
            set { PropertySetter(value, newValue => _useId = newValue); }
        }

        public string UserName
        {
            get { return _userName; }
            set { PropertySetter(value, newValue => _userName = newValue); }
        }

        public string Password
        {
            get { return _password; }
            set { PropertySetter(value, newValue => _password = newValue); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { PropertySetter(value, newValue => _firstName = newValue); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { PropertySetter(value, newValue => _lastName = newValue); }
        }

        public string Email
        {
            get { return _email; }
            set { PropertySetter(value, newValue => _email = newValue); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { PropertySetter(value, newValue => _isActive = newValue); }
        }

        public string Phone
        {
            get { return _phone; }
            set { PropertySetter(value, newValue => _phone = newValue); }
        }

        public ObservableCollection<GroupViewModel> Groups
        {
            get { return _groups; }
            set { PropertySetter(value, newValue => _groups = newValue); }
        }

        public ObservableCollection<OrganizationViewModel> Organizations
        {
            get { return _organizations; }
            set { PropertySetter(value, newValue => _organizations = newValue); }
        }

        public bool IsADUser
        {
            get { return _isADUser; }
            set { PropertySetter(value, newValue => _isADUser = newValue); }
        }

        public User User
        {
            get { return _user; }
            set { PropertySetter(value, newValue => _user = newValue); }
        }

        public bool KeepLoggedIn
        {
            get { return _keepLoggedIn; }
            set { PropertySetter(value, newValue => _keepLoggedIn = newValue); }
        }
        public string ApiKey
        {
            get { return _apiKey; }
            set { PropertySetter(value, newValue => _apiKey = newValue); }
        }
        public int LanguageID
        {
            get { return _languageID; }
            set { PropertySetter(value, newValue => _languageID = newValue); }
        }
        #endregion
    }
}