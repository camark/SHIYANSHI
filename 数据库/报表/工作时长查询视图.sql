select al.ID,----1����
      oti.ORDER_NUMBER,----2ί�е���
      '',----3������λ
      oti.CERTIFICATE_ENTERPRISE,----4֤�鵥λ
      oti.ACCEPT_ORGNIZATION,--5����λ 
      adi.APPLIANCE_NAME, --6��������
      '',--7��������
      adi.VERSION, --8�����ͺ�
      adi.FORMAT,--9���߹��
      adi.FACTORY_NUM, --10�������
      adi.NUM, --11����
      ps.REPORTNUMBER,--12֤��/������
      al.UNDERTAKE_LABORATORYID, --13ʵ����
      ps.CHECKERID, --14�춨/У׼Ա
      ps.DETECTERID,--15����Ա
      '',--16��������
      oti.CREATETIME, --17�ͼ�����
      al.RECEIVETIME,--18ʵ���ҽ���ʱ��
      '',--19�춨�������
      ps.AUDITTIME,--20�������
      ps.APPROVALDATE,--21��׼����
      '',--22����ȡʱ��
      '',--23�춨ʱ��
      '',--24���ʱ��
      '',--25��׼ʱ��
      '',--26��ʱ��
      adi.REMARKS--27��ע  
                  
  from PREPARE_SCHEME ps
inner join APPLIANCE_LABORATORY al
on al.PREPARE_SCHEMEID=ps.id
inner join APPLIANCE_DETAIL_INFORMATION adi
on al.appliance_detail_informationid=adi.id
inner join ORDER_TASK_INFORMATION oti
on oti.id=adi.order_task_informationid