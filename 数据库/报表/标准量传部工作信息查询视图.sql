
  CREATE OR REPLACE FORCE VIEW TENGFEI.VBIAOZHUNLIANGCHUANGONGZHUO 
  (
  ID,----1����
  WEITUODANWEI,----2ί�е���
  SUOSHUDANWEI, ----3������λ
  ZHENGSHUDANWEI, ----4֤�鵥λ
  SONGJIANDANWEI, ---5�ͼ쵥λ
  SHOULIDANWEI,----6����λ
  CHUCHANGRIQI,----7��������
  QIJUMINGCHENG,----8��������
  SHENGCHANCHANGJIA,----9��������
  QIJUXINGHAO, ----10�����ͺ�
  QIJUGUIGE,----11���߹��
  CHUCHANGBIANHAO,----12�������
  SHULIANG,----13����
  SONGJIANRIQI,----14�ͼ�����
  SONGJIANREN, ----15�ͼ���
  JIESHOUREN,--16������
  SHIYANSHI, --17ʵ����
  SHIYANSHIJIESHOUSHIJIAN, --18ʵ���ҽ���ʱ��
  JIANDINGRIQI, ----19�춨����
  JIANDINGXIAOZHUNYUAN,----20�춨/У׼Ա
  HEYANYUAN,----21����Ա
  ZHENGSHUHAOLEIBIE,--22֤������
  ZHENGSHUBAOGAOBIANHAO,----23֤��/������
  ZHENGSHULEIBIE, ----24֤�����
  BAOGAOLEIBIE, ----25�������
  SHOUQUANZIZHI,----26��Ȩ/����
  QIJUZHUANGTAI,--27����״̬
  YOUXIAOQIZHI,--28��Ч����
  BAOGAOSHENPITONGGUORIQI,--29��������ͨ������
  MOCHONGCHANGSHU,--30����״̬
  SONGJIANYUEDU,--31�ͼ��¶�
  JIANDINGSHIJIAN, --32�춨ʱ��
  JIANDINGYUEDU,--33�춨�¶�
  BAOGAOSHIJIAN,--34����ʱ��
  BAOGAOYUEDU, --35�����¶�
  GONHZUOSHIJIAN,--36����ʱ��
  ZONGSHIJIAN,--37��ʱ��
  BEIZHU--38��ע
  ) AS 
  select al.ID,----1����
    oti.ORDER_NUMBER,----2ί�е���
   oti.CERTIFICATE_ENTERPRISEHELLD,----3������λ
    oti.CERTIFICATE_ENTERPRISE,----4֤�鵥λ
     oti.INSPECTION_ENTERPRISE,---5�ͼ쵥λ
    oti.ACCEPT_ORGNIZATION,----6����λ
  '',----7��������
  adi.APPLIANCE_NAME,----8��������
    '',----9��������
   adi.VERSION,----10�����ͺ�
    adi.FORMAT,----11���߹��
  adi.FACTORY_NUM,----12�������
   adi.NUM,----13����
   oti.CREATETIME,----14�ͼ�����
 oti.CONTACTS,----15�ͼ���
   oti.CREATEPERSON, --16������
    al.UNDERTAKE_LABORATORYID, --17ʵ����
  al.RECEIVETIME,--18ʵ���ҽ���ʱ��
  ps.CALIBRATION_DATE,----19�춨����
  ps.CHECKERID,----20�춨/У׼Ա
  ps.DETECTERID,----21����Ա
ps.CERTIFICATE_CATEGORY,--22֤������
ps.REPORTNUMBER,----23֤��/������
  ps.CERTIFICATE_CATEGORY,----24֤�����
  ps.REPORT_CATEGORY,----25�������
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
  end as SHOUQUANZIZHI, ----26��Ȩ/����
al.ORDER_STATUS,--27����״̬
 add_months(CALIBRATION_DATE,12*VALIDITY_PERIOD) as YOUXIAOQIZHI,--28��Ч����
    ps.APPROVALDATE,--29��������ͨ������
  ps.REPORTSTATUS, --30����״̬
  (
  case
  when to_number(to_char(oti.CREATETIME,'MM'))=12 
  then to_number(to_char(oti.CREATETIME,'MM')) 
  when to_number(to_char(oti.CREATETIME,'MM'))<12
  and to_number(to_char(oti.CREATETIME,'DD'))<16
  then to_number(to_char(oti.CREATETIME,'MM'))
  when to_number(to_char(oti.CREATETIME,'MM'))<12
  and to_number(to_char(oti.CREATETIME,'DD'))>15
  then to_number(to_char(oti.CREATETIME,'MM'))+1
  else null end
  ) 
  as SONGJIANYUEDU,--31�ͼ��¶�
 '',--32�춨ʱ��
 '',--33�춨�¶�
 '',--34����ʱ��
 '',--35�����¶�
 '',--36����ʱ��
 '',--37��ʱ��
  adi.REMARKS--38��ע
    
  from PREPARE_SCHEME ps
inner join APPLIANCE_LABORATORY al
on al.PREPARE_SCHEMEID=ps.id
inner join APPLIANCE_DETAIL_INFORMATION adi
on al.appliance_detail_informationid=adi.id
inner join ORDER_TASK_INFORMATION oti
on oti.id=adi.order_task_informationid;
 
