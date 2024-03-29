﻿using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Trinity.Config;
using Trinity.Config.Combat;
using Trinity.Config.Loot;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Bot;

namespace Trinity.UI.UIComponents
{
    /// <summary>
    /// ViewModel injected to Configuration Window 
    /// </summary>
    public class ConfigViewModel
    {
        private TrinitySetting _Model;
        private TrinitySetting _OriginalModel;

        public bool DebugVisibility
        {
            get
            {
                if (V.S("DebugVisibility") == "Visible")
                    return true;

                return false;
            }
        }

        internal static Grid MainWindowGrid()
        {
            return (System.Windows.Application.Current.MainWindow.Content as Grid);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigViewModel" /> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ConfigViewModel(TrinitySetting model)
        {
            try
            {
                _OriginalModel = model;
                _Model = new TrinitySetting();
                _OriginalModel.CopyTo(_Model);
                InitializeResetCommand();
                SaveCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                if (Trinity.StashRule == null && _Model.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
                                                {
                                                    // Load interpreter for the first time if needed
                                                    Trinity.StashRule = new ItemRules.Interpreter();
                                                }

                                                _Model.CopyTo(_OriginalModel);
                                                _OriginalModel.Save();

                                                if (_Model.Advanced.TPSEnabled != _OriginalModel.Advanced.TPSEnabled)
                                                    BotManager.SetBotTicksPerSecond();

                                                if (_Model.Advanced.UnstuckerEnabled != _OriginalModel.Advanced.UnstuckerEnabled)
                                                    BotManager.SetUnstuckProvider();

                                                if (_Model.Loot.ItemFilterMode != _OriginalModel.Loot.ItemFilterMode)
                                                    BotManager.SetItemManagerProvider();

                                                CacheData.FullClear();
                                                UsedProfileManager.SetProfileInWindowTitle();

                                                UILoader.CloseWindow();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception in UI SaveCommand {0}", ex);
                                            }
                                        });
                DumpBackpackCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nDumping Backpack Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.Backpack);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception dumping Backpack: {0}", ex);
                                            }
                                        });
                DumpQuickItemsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nQuick Dumping Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpQuickItems();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception Quick Dumping: {0}", ex);
                                            }
                                        });
                DumpAllItemsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nDumping ALL Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.All);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception Dumping ALL Items: {0}", ex);
                                            }
                                        });
                DumpMerchantItemsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nDumping Merchant Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.Merchant);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception dumping Merchant: {0}", ex);
                                            }
                                        });
                DumpEquippedCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nDumping Equipped Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.Equipped);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception dumping Equipped: {0}", ex);
                                            }
                                        });
                DumpGroundItemsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Logger.Log(
                                                    "\n############################################\n"
                                                    + "\nDumping Ground Items. This will hang your client. Please wait....\n"
                                                    + "##########################");
                                                UILoader.CloseWindow();
                                                TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.Ground);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "Exception dumping Ground: {0}", ex);
                                            }
                                        });
                DumpStashCommand = new RelayCommand(
                                      (parameter) =>
                                      {
                                          try
                                          {
                                              Logger.Log(
                                                  "\n############################################\n"
                                                  + "\nDumping Stash Items. This will hang your client. Please wait....\n"
                                                  + "##########################");
                                              UILoader.CloseWindow();
                                              TrinityItemManager.DumpItems(TrinityItemManager.DumpItemLocation.Stash);
                                          }
                                          catch (Exception ex)
                                          {
                                              Logger.Log(LogCategory.UserInformation, "Exception dumping Stash: {0}", ex);
                                          }
                                      });
                TestScoreCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                Trinity.TestScoring();
                                                //UILoader.CloseWindow();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "{0}", ex);
                                            }
                                        });
                OrderStashCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                //TownRun.SortStash();
                                                UILoader.CloseWindow();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log(LogCategory.UserInformation, "{0}", ex);
                                            }

                                        });
                HelpLinkCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            string link = parameter as string;
                                            if (!string.IsNullOrWhiteSpace(link))
                                            {
                                                Process.Start(link);
                                            }
                                        });
                ConfigureLootToHunting = new RelayCommand(
                                        (parameter) =>
                                        {
                                            ConfigHuntingLoot();
                                        });
                ConfigureLootToQuesting = new RelayCommand(
                                        (parameter) =>
                                        {
                                            ConfigQuestingLoot();
                                        });
                LoadItemRuleSetCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                LoadItemRulesPath();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception in LoadItemRuleSetCommand: {0}", ex);
                                            }
                                        });
                OpenTVarsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                V.ValidateLoad();
                                                TVarsViewModel.CreateWindow().Show();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception in OpenTVarsCommand: {0}", ex);
                                            }
                                        });
                UseGlobalConfigFileCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            DialogResult rusure = MessageBox.Show("This will force all bots running under this Demonbuddy directory to use a shared configuration file.\n"
                                                + "You can undo this by removing the Trinity.xml file under your Demonbuddy settings directory. \n"
                                                + "Are you sure?",
                                            "Confirm global settings",
                                            MessageBoxButtons.OKCancel);

                                            if (rusure == DialogResult.OK)
                                            {
                                                Trinity.Settings.Save(true);
                                            }
                                        });
                DumpSkillsCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            PlayerInfoCache.DumpPlayerSkills();

                                            UILoader.CloseWindow();
                                        });
            }
            catch (Exception ex)
            {
                Logger.LogError("Error creating Trinity View Model {0}", ex);
            }
        }

        /// <summary>
        /// Opens the TVars config window
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpSkillsCommand
        {
            get;
            private set;
        }


        /// <summary>
        /// Opens the TVars config window
        /// </summary>
        /// <value>The save command.</value>
        public ICommand OpenTVarsCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Reloads item rules
        /// </summary>
        /// <value>The save command.</value>
        public ICommand LoadItemRuleSetCommand
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand HelpLinkCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand ConfigureLootToQuesting
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the HelpLink command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand ConfigureLootToHunting
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand TestScoreCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpBackpackCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpQuickItemsCommand
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpAllItemsCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpMerchantItemsCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpEquippedCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpGroundItemsCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the test score command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand DumpStashCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the order stash command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand OrderStashCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>The save command.</value>
        public ICommand SaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Misc Tab.
        /// </summary>
        /// <value>The reset command for Misc Tab.</value>
        public ICommand ResetMiscCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Barbarian Tab.
        /// </summary>
        /// <value>The reset command for Barbarian Tab.</value>
        public ICommand ResetBarbCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Crusader Tab.
        /// </summary>
        /// <value>The reset command for Barbarian Tab.</value>
        public ICommand ResetCrusaderCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Monk Tab.
        /// </summary>
        /// <value>The reset command for Monk Tab.</value>
        public ICommand ResetMonkCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Wizard Tab.
        /// </summary>
        /// <value>The reset command for Wizard Tab.</value>
        public ICommand ResetWizardCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Witch Doctor Tab.
        /// </summary>
        /// <value>The reset command for Witch Doctor Tab.</value>
        public ICommand ResetWitchDoctorCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Demon Hunter Tab.
        /// </summary>
        /// <value>The reset command for Demon Hunter Tab.</value>
        public ICommand ResetDemonHunterCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for World Object Tab.
        /// </summary>
        /// <value>The reset command for World Object Tab.</value>
        public ICommand ResetWorldObjectCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Town Run Tab.
        /// </summary>
        /// <value>The reset command for Town Run Tab.</value>
        public ICommand ResetTownRunCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Items Tab.
        /// </summary>
        /// <value>The reset command for Items Tab.</value>
        public ICommand ResetItemCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Items Tab.
        /// </summary>
        /// <value>The reset command for Items Tab.</value>
        public ICommand ResetItemRulesCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Reloads Script Rules
        /// </summary>
        public ICommand ReloadScriptRulesCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Advanced Tab.
        /// </summary>
        /// <value>The reset command for Advanced Tab.</value>
        public ICommand ResetAdvancedCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Mobile Tab.
        /// </summary>
        /// <value>The reset command for Mobile Tab.</value>
        public ICommand ResetNotificationCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for Logs Tab.
        /// </summary>
        /// <value>The reset command for Logs Tab.</value>
        public ICommand ResetLogCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reset command for all settings.
        /// </summary>
        /// <value>The reset command for all settings.</value>
        public ICommand ResetAllCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Makes Trinity use a "Global" configuration file
        /// </summary>
        /// <value>The reset command for all settings.</value>
        public ICommand UseGlobalConfigFileCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Misc Configuration Model.
        /// </summary>
        /// <value>The Misc Configuration Model.</value>
        public MiscCombatSetting Misc
        {
            get
            {
                return _Model.Combat.Misc;
            }
        }

        /// <summary>
        /// Gets the Advanced Configuration Model.
        /// </summary>
        /// <value>The Advanced Configuration Model.</value>
        public AdvancedSetting Advanced
        {
            get
            {
                return _Model.Advanced;
            }
        }

        /// <summary>
        /// Gets the Avoidance Configuration Model.
        /// </summary>
        /// <value>The Avoidance Configuration Model.</value>
        public AvoidanceRadiusSetting Avoid
        {
            get
            {
                return _Model.Combat.AvoidanceRadius;
            }
        }

        /// <summary>
        /// Gets the Barbarian Configuration Model.
        /// </summary>
        /// <value>The Barbarian Configuration Model.</value>
        public BarbarianSetting Barb
        {
            get
            {
                return _Model.Combat.Barbarian;
            }
        }

        /// <summary>
        /// Gets the Barbarian Configuration Model.
        /// </summary>
        /// <value>The Barbarian Configuration Model.</value>
        public CrusaderSetting Crusader
        {
            get
            {
                return _Model.Combat.Crusader;
            }
        }

        /// <summary>
        /// Gets the Demon Hunter Configuration Model.
        /// </summary>
        /// <value>The Demon Hunter Configuration Model.</value>
        public DemonHunterSetting DH
        {
            get
            {
                return _Model.Combat.DemonHunter;
            }
        }

        /// <summary>
        /// Gets the Monk Configuration Model.
        /// </summary>
        /// <value>The Monk Configuration Model.</value>
        public MonkSetting Monk
        {
            get
            {
                return _Model.Combat.Monk;
            }
        }

        /// <summary>
        /// Gets the Witch Doctor Configuration Model.
        /// </summary>
        /// <value>The Witch Doctor Configuration Model.</value>
        public WitchDoctorSetting WD
        {
            get
            {
                return _Model.Combat.WitchDoctor;
            }
        }

        /// <summary>
        /// Gets the Wizard Configuration Model.
        /// </summary>
        /// <value>The Wizard Configuration Model.</value>
        public WizardSetting Wiz
        {
            get
            {
                return _Model.Combat.Wizard;
            }
        }

        /// <summary>
        /// Gets the World Object Configuration Model.
        /// </summary>
        /// <value>The World Object Configuration Model.</value>
        public WorldObjectSetting WorldObject
        {
            get
            {
                return _Model.WorldObject;
            }
        }

        /// <summary>Gets the TownRun Configuration Model.</summary>
        /// <value>The TownRun Configuration Model.</value>
        public TownRunSetting TownRun
        {
            get
            {
                return _Model.Loot.TownRun;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public PickupSetting Pickup
        {
            get
            {
                return _Model.Loot.Pickup;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public ItemSetting Loot
        {
            get
            {
                return _Model.Loot;
            }
        }
        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public ItemRuleSetting ItemRules
        {
            get
            {
                return _Model.Loot.ItemRules;
            }
        }

        /// <summary>
        /// Gets the Pickup Configuration Model.
        /// </summary>
        /// <value>The Pickup Configuration Model.</value>
        public NotificationSetting Notification
        {
            get
            {
                return _Model.Notification;
            }
        }

        /// <summary>
        /// Initializes the Reset commands.
        /// </summary>
        private void InitializeResetCommand()
        {
            try
            {
                ResetMiscCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.Misc.Reset();
                                        });
                ResetBarbCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.Barbarian.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetCrusaderCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.Crusader.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetMonkCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.Monk.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetWizardCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.Wizard.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetWitchDoctorCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.WitchDoctor.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetDemonHunterCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Combat.DemonHunter.Reset();
                                            _Model.Combat.AvoidanceRadius.Reset();
                                        });
                ResetWorldObjectCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.WorldObject.Reset();
                                        });
                ResetItemCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Loot.Pickup.Reset();
                                        });
                ResetItemRulesCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Loot.ItemRules.Reset();
                                        });
                ReloadScriptRulesCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            try
                                            {
                                                _Model.CopyTo(_OriginalModel);
                                                _OriginalModel.Save();
                                                if (Trinity.StashRule == null)
                                                    Trinity.StashRule = new ItemRules.Interpreter();

                                                if (Trinity.StashRule != null)
                                                {
                                                    BotMain.PauseWhile(Trinity.StashRule.reloadFromUI);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log("Exception in ReloadScriptRulesCommand: {0}", ex);
                                            }
                                        }
                                        );
                ResetTownRunCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Loot.TownRun.Reset();
                                        });
                ResetAdvancedCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Advanced.Reset();
                                        });
                ResetNotificationCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Notification.Reset();
                                        });

                ResetAllCommand = new RelayCommand(
                                        (parameter) =>
                                        {
                                            _Model.Reset();
                                        });
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception initializing commands {0}", ex);
            }
        }

        private void ConfigQuestingLoot()
        {
            try
            {
                _Model.Loot.ItemFilterMode = ItemFilterMode.TrinityOnly;
                _Model.Loot.Pickup.ArmorBlueLevel = 1;
                _Model.Loot.Pickup.ArmorYellowLevel = 1;
                _Model.Loot.Pickup.WeaponBlueLevel = 1;
                _Model.Loot.Pickup.WeaponYellowLevel = 1;
                _Model.Loot.Pickup.JewelryBlueLevel = 1;
                _Model.Loot.Pickup.JewelryYellowLevel = 1;
                _Model.Loot.Pickup.LegendaryLevel = 1;
                _Model.Loot.Pickup.GemLevel = 15;
                _Model.Loot.Pickup.GemType = TrinityGemType.All;
                _Model.Loot.Pickup.PickupBlueFollowerItems = false;
                _Model.Loot.Pickup.PickupYellowFollowerItems = true;
                _Model.Loot.Pickup.PickupGold = true;
                _Model.Loot.Pickup.MinimumGoldStack = 100;
                _Model.Loot.Pickup.MiscItemQuality = TrinityItemQuality.Common;
                _Model.Loot.Pickup.PotionCount = 98;
                _Model.Loot.Pickup.Plans = true;
                _Model.Loot.Pickup.CraftTomes = true;
                _Model.Loot.Pickup.PickupLowLevel = true;
                _Model.Loot.Pickup.LegendaryPlans = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error configuring questing settings {0}", ex);
            }
        }

        private void ConfigHuntingLoot()
        {
            try
            {
                _Model.Loot.ItemFilterMode = ItemFilterMode.TrinityOnly;
                _Model.Loot.Pickup.ArmorBlueLevel = 0;
                _Model.Loot.Pickup.ArmorYellowLevel = 0;
                _Model.Loot.Pickup.WeaponBlueLevel = 0;
                _Model.Loot.Pickup.WeaponYellowLevel = 0;
                _Model.Loot.Pickup.JewelryBlueLevel = 0;
                _Model.Loot.Pickup.JewelryYellowLevel = 0;
                _Model.Loot.Pickup.LegendaryLevel = 70;
                _Model.Loot.Pickup.GemLevel = 15;
                _Model.Loot.Pickup.GemType = TrinityGemType.All;
                _Model.Loot.Pickup.PickupBlueFollowerItems = false;
                _Model.Loot.Pickup.PickupYellowFollowerItems = false;
                _Model.Loot.Pickup.PickupGold = true;
                _Model.Loot.Pickup.MinimumGoldStack = 600;
                _Model.Loot.Pickup.MiscItemQuality = TrinityItemQuality.Legendary;
                _Model.Loot.Pickup.PotionCount = 98;
                _Model.Loot.Pickup.Plans = true;
                _Model.Loot.Pickup.CraftTomes = true;
                _Model.Loot.Pickup.PickupLowLevel = true;
                _Model.Loot.Pickup.LegendaryPlans = true;

                _Model.Loot.TownRun.ForceSalvageRares = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error configuring hunting settings {0}", ex);
            }

        }

        private void LoadItemRulesPath()
        {
            try
            {
                var folderDialog = new FolderBrowserDialog();

                folderDialog.ShowNewFolderButton = false;
                folderDialog.SelectedPath = FileManager.ItemRulePath;

                DialogResult result = folderDialog.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result == DialogResult.OK)
                {
                    // Open document 
                    string directory = folderDialog.SelectedPath;

                    if (directory != FileManager.ItemRulePath)
                    {
                        ItemRules.ItemRuleSetPath = directory;

                        Logger.Log(TrinityLogLevel.Info, LogCategory.Configuration, "Loaded ItemRule Set {0}", ItemRules.ItemRuleSetPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in LoadItemRulesPath: {0}", ex);
            }
        }
    }
}
