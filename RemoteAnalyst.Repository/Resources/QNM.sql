CREATE TABLE `QNM_About` (
  `NODE` varchar(50) DEFAULT NULL,
  `FROM` datetime(6) DEFAULT NULL,
  `TO` datetime(6) DEFAULT NULL,
  `INTERVAL` varchar(50) DEFAULT NULL,
  `VERSION` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_CLIMDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `CLIM Name` varchar(50) DEFAULT NULL,
  `Sent Bytes` double DEFAULT NULL,
  `Received Bytes` double DEFAULT NULL,
  `Errors` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_CLIMSummary` (
  `CLIM Name` varchar(50) DEFAULT NULL,
  `Total Bytes` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_ExpandPathDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Device Name` varchar(50) DEFAULT NULL,
  `Sent Packets` double DEFAULT NULL,
  `Received Packets` double DEFAULT NULL,
  `Sent Forwards` double DEFAULT NULL,
  `Received Forwards` double DEFAULT NULL,
  `L4 Packets Discarded` double DEFAULT NULL,
  `Avg.Packets/Frame Sent` double DEFAULT NULL,
  `Avg.Bytes/Frame Sent` double DEFAULT NULL,
  `Avg.Packets/Frame Received` double DEFAULT NULL,
  `Avg.Bytes/Frame Received` double DEFAULT NULL,
  `Transmit Timeouts` double DEFAULT NULL,
  `Retransmit Timeouts` double DEFAULT NULL,
  `Retransmit Packets` double DEFAULT NULL,
  `OOS Usage` double DEFAULT NULL,
  `OOS Timeouts` double DEFAULT NULL,
  `L4 Sent Ack` double DEFAULT NULL,
  `L4 Sent NOT Ack` double DEFAULT NULL,
  `L4 Received Ack` double DEFAULT NULL,
  `L4 Received NOT Ack` double DEFAULT NULL,
  `L3 Misc Bad Packets` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_ExpandPathSummary` (
  `Device Name` varchar(50) DEFAULT NULL,
  `Total Packets` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_ProbeRoundTripDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Selected System` varchar(50) DEFAULT NULL,
  `Number of Systems` double DEFAULT NULL,
  `List of Systems` varchar(500) DEFAULT NULL,
  `Trip Time (ms)` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_SLSADetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `PIF Name` varchar(50) DEFAULT NULL,
  `Type` varchar(50) DEFAULT NULL,
  `Sent Octets` double DEFAULT NULL,
  `Received Octets` double DEFAULT NULL,
  `Sent Errors` double DEFAULT NULL,
  `Received Errors` double DEFAULT NULL,
  `Collision Frames` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_SLSASummary` (
  `PIF Name` varchar(50) DEFAULT NULL,
  `Total Octets` double DEFAULT NULL,
  `Line Speed Mbits/sec` double DEFAULT NULL,
  `Auto Negotiation` double DEFAULT NULL,
  `Duplex Mode` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_SysDiagrams` (
  `name` varchar(160) NOT NULL,
  `principal_id` int(11) NOT NULL,
  `diagram_id` int(11) NOT NULL AUTO_INCREMENT,
  `version` int(11) DEFAULT NULL,
  `definition` longblob,
  PRIMARY KEY (`diagram_id`),
  UNIQUE KEY `UK_principal_name` (`principal_id`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPPacketsDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Process Name` varchar(50) DEFAULT NULL,
  `TCP Sent Packets` double DEFAULT NULL,
  `TCP Received Packets` double DEFAULT NULL,
  `TCP Received Out of Order Packets` double DEFAULT NULL,
  `UDP Total Input Packets` double DEFAULT NULL,
  `UDP Total Output Packets` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPPacketsSummary` (
  `Process Name` varchar(50) DEFAULT NULL,
  `Total Packets` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPProcessDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Process Name` varchar(50) DEFAULT NULL,
  `Sent Bytes` double DEFAULT NULL,
  `Received Bytes` double DEFAULT NULL,
  `Received Out of Order bytes` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPProcessSummary` (
  `Process Name` varchar(50) DEFAULT NULL,
  `Total Bytes` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPSubnetDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Subnet Process Name (IP Address)` varchar(100) DEFAULT NULL,
  `Out Packets` double DEFAULT NULL,
  `In Packets` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPSubnetSummary` (
  `Subnet Process Name` varchar(50) DEFAULT NULL,
  `Total Packets` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPv6Detail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `Monitor Name` varchar(50) DEFAULT NULL,
  `Sent Bytes` double DEFAULT NULL,
  `Received Bytes` double DEFAULT NULL,
  `Received Out of Order bytes` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPv6SubnetDetail` (
  `Date Time` datetime(6) DEFAULT NULL,
  `SUBNET Monitor Name (IP Address)` varchar(50) DEFAULT NULL,
  `Out Packets` double DEFAULT NULL,
  `In Packets` double DEFAULT NULL,
  `Reset` char(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPv6SubnetSummary` (
  `SUBNET Monitor Name (IP Address)` varchar(50) DEFAULT NULL,
  `Total Packets` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
CREATE TABLE `QNM_TCPv6Summary` (
  `Monitor Name` varchar(50) DEFAULT NULL,
  `Total Bytes` double DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
