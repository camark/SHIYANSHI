
  CREATE OR REPLACE FORCE VIEW TENGFEI.VZHENGSHULEIBEITONGJIFENXI 
  (
  ID, ----1���� 
  SUOSHUDANWEI,----2������λ
  ZHENGSHUDANWEI, ----3֤�鵥λ
  SHOULIDANWEI,----4����λ
  PIZHUNJIELUN, ----5��׼����
  PIZHUNSHIJIAN, ----6��׼ʱ��
  SHOUQUANZIZHI,----7��Ȩ/����
  ZHENGSHULEIBIE,----8֤�����
  BAOGAOLEIBIE, ----9�������
  BAOGAOSHULIANG----10��������
  ) AS 
  select al.ID,----1����  
    '',----2������λ
    oti.CERTIFICATE_ENTERPRISE,----3֤�鵥λ
    oti.ACCEPT_ORGNIZATION,----4����λ
  ps.APPROVALISAGGREY,----5��׼����
  ps.APPROVALDATE,----6��׼ʱ��
  CASE
    when ps.QUALIFICATIONS     ='����λ�񱱾������������ල��ר�������Ȩ��֤���ţ����������ƣ�2012��006��'
    and ps.CERTIFICATE_CATEGORY='�춨'
    and oti.ACCEPT_ORGNIZATION  ='����������ѧ�о�Ժ�������ι�˾'
    THEN '������Ȩ�춨'
    WHEN ps.qualifications     ='/'
    and ps.CERTIFICATE_CATEGORY='�춨'
    and oti.ACCEPT_ORGNIZATION  ='����������ѧ�о�Ժ�������ι�˾'
    THEN '�춨'
    WHEN ps.qualifications     ='��ʵ���һ��й��ϸ����������Ͽ�ίԱ��CNAS���Ͽ�֤�飬֤���No.L0394'
    and ps.CERTIFICATE_CATEGORY='У׼'
    and oti.ACCEPT_ORGNIZATION  ='����������ѧ�о�Ժ�������ι�˾'
    THEN 'CNASУ׼'
    WHEN ps.qualifications     ='/'
    and ps.CERTIFICATE_CATEGORY='У׼'
    and oti.ACCEPT_ORGNIZATION  ='����������ѧ�о�Ժ�������ι�˾'
    THEN 'У׼'
    WHEN ps.qualifications     ='����λ��ӱ������������ල��ר�������Ȩ��֤���ţ����������ƣ�2014��D033��'
    and ps.CERTIFICATE_CATEGORY='�춨'
    and oti.ACCEPT_ORGNIZATION  ='�����������޹�˾��������'
    THEN '�ӱ���Ȩ�춨'
    WHEN ps.qualifications     ='/'
    and ps.CERTIFICATE_CATEGORY='�춨'
    and oti.ACCEPT_ORGNIZATION  ='����������ѧ�о�Ժ�������ι�˾'
    THEN '�춨'
    WHEN ps.qualifications     ='��ʵ���һ��й��ϸ����������Ͽ�ίԱ��CNAS���Ͽ�֤�飬֤���No.L0394'
    and ps.CERTIFICATE_CATEGORY='У׼'
    and oti.ACCEPT_ORGNIZATION  ='�����������޹�˾��������'
    THEN 'CNASУ׼'
    WHEN ps.qualifications     ='/'
    and ps.CERTIFICATE_CATEGORY='У׼'
    and oti.ACCEPT_ORGNIZATION  ='�����������޹�˾��������'
    THEN 'У׼'
    ELSE ''
  end as SHOUQUANZIZHI, ----7��Ȩ/����
    ps.CERTIFICATE_CATEGORY,----8֤�����
  ps.REPORT_CATEGORY,----9�������
  1----10��������
    
  from PREPARE_SCHEME ps
inner join APPLIANCE_LABORATORY al
on al.PREPARE_SCHEMEID=ps.id
inner join APPLIANCE_DETAIL_INFORMATION adi
on al.appliance_detail_informationid=adi.id
inner join ORDER_TASK_INFORMATION oti
on oti.id=adi.order_task_informationid;
 
