﻿using SalamanderWnmp.Tool;
using System;
using System.ServiceProcess;
using System.Windows.Controls;

namespace SalamanderWnmp.Programs
{
    class MysqlProgram : BaseProgram
    {
        private readonly ServiceController mysqlController = new ServiceController();
        public const string ServiceName = "mysql-salamander";

        public MysqlProgram()
        {
            mysqlController.MachineName = Environment.MachineName;
            mysqlController.ServiceName = ServiceName;
        }

        /// <summary>
        /// 移除服务
        /// </summary>
        public void RemoveService()
        {
            StartProcess("cmd.exe", stopArgs, true);
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        public void InstallService()
        {
            StartProcess(exeName, startArgs, true);
        }


        /// <summary>
        /// 服务是否存在
        /// </summary>
        /// <returns></returns>
        public bool ServiceExists()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (var service in services) {
                if (service.ServiceName == ServiceName)
                    return true;
            }
            return false;
        }

        public override void Start()
        {
            if (IsRunning())
                return;
            try {
                mysqlController.Start();
                Log.wnmp_log_notice("Started " + progName, progLogSection);
            } catch (Exception ex) {
                Log.wnmp_log_error("Start(): " + ex.Message, progLogSection);
            }
        }

        public override void Stop()
        {
            if(!IsRunning())
            {
                return;
            }
            try {
                mysqlController.Stop();
                mysqlController.WaitForStatus(ServiceControllerStatus.Stopped);
                Log.wnmp_log_notice("Stopped " + progName, progLogSection);
            } catch (Exception ex) {
                Log.wnmp_log_error("Stop(): " + ex.Message, progLogSection);
            }
        }


        /// <summary>
        /// 通过ServiceController判断服务是否在运行
        /// </summary>
        /// <returns></returns>
        public override bool IsRunning()
        {
            mysqlController.Refresh();
            try
            {
                return mysqlController.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                return false;
            }
        }

        public override void Setup()
        {
            this.exeName = Common.APP_STARTUP_PATH + String.Format("{0}/bin/mysqld.exe", Common.Settings.MysqlDirName.Value);
            this.procName = "mysqld";
            this.progName = "mysql";
            this.workingDir = Common.APP_STARTUP_PATH + Common.Settings.MysqlDirName.Value;
            this.progLogSection = Log.LogSection.WNMP_MARIADB;
            this.startArgs = "--install-manual " + MysqlProgram.ServiceName + " --defaults-file=\"" +
                Common.APP_STARTUP_PATH + String.Format("\\{0}\\my.ini\"", Common.Settings.MysqlDirName.Value);
            this.stopArgs = "/c sc delete " + MysqlProgram.ServiceName;
            this.killStop = true;
            this.confDir = "/mysql/";
            this.logDir = "/mysql/data/";
        }
    }
}
