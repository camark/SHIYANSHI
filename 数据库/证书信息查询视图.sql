select al."ID",----1����
  oti.INSPECTION_ENTERPRISE,---2�ͼ쵥λ
  oti."CERTIFICATE_ENTERPRISE",----3֤�鵥λ
  oti."ACCEPT_ORGNIZATION",----4����λ
  '',----5��������
  adi."APPLIANCE_NAME",----6��������
  '',----7��������
  adi."VERSION",----8�����ͺ�
  adi."FACTORY_NUM",----9�������
  ps."ACCURACY_GRADE",----10׼ȷ�ȵȼ�
  ps."CALIBRATION_DATE",----11�춨����
  ps."TEMPERATURE",----12�¶ȣ��棩
  ps."HUMIDITY",----13���ʪ�ȣ�%��
  ps."PULSE_CONSTANT",----14���峣��(imp/kWh)
  adi."FORMAT",----15���߹��
  ps."CHECKERID",----16�춨/У׼Ա
  ps."DETECTERID",----17����Ա
  ps."VALIDITY_PERIOD",----18��Ч�ڣ��꣩
  add_months(CALIBRATION_DATE,12*VALIDITY_PERIOD) as YOUXIAOQIZHI,----19��Ч����
  ps."REPORTNUMBER",----20֤��/������
  ps."CERTIFICATE_CATEGORY",----21֤�����
  ps."REPORT_CATEGORY",----22�������
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
  end as SHOUQUANZIZHI, ----23��Ȩ/����
  (SELECT REPORTTORECEVESTATE from REPORTCOLLECTION where PREPARE_SCHEMEID=ps.id)----24����״̬
  --rn."REPORTTORECEVESTATE",----24����״̬
  oti."CERTIFICATE_ENTERPRISEHELLD",----25������λ
  oti."ORDER_NUMBER",----26ί�е���
  adi."REMARKS"----27��ע

  
  from PREPARE_SCHEME ps
inner join APPLIANCE_LABORATORY al
on al.PREPARE_SCHEMEID=ps.id
--inner join REPORTCOLLECTION   rn
--on rn.PREPARE_SCHEMEID=ps.id
inner join APPLIANCE_DETAIL_INFORMATION adi
on al.appliance_detail_informationid=adi.id
inner join ORDER_TASK_INFORMATION oti
on oti.id=adi.order_task_informationid