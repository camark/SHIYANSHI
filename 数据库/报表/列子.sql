
select UNDERTAKE_LABORATORYID,
sum(case when conclusion='�ϸ�' then icount end) �ϸ�,
sum(case when conclusion='���ϸ�' then icount end) ���ϸ�
from (
select UNDERTAKE_LABORATORYID,'�ϸ�' conclusion,COUNT(ps.id) icount from APPLIANCE_LABORATORY al
left join PREPARE_SCHEME ps on al.PREPARE_SCHEMEID=ps.id
and ps.conclusion='�ϸ�'
group by UNDERTAKE_LABORATORYID
union all
select UNDERTAKE_LABORATORYID,'���ϸ�' conclusion,COUNT(ps.id) icount  from APPLIANCE_LABORATORY al
left join PREPARE_SCHEME ps on al.PREPARE_SCHEMEID=ps.id
and ps.conclusion='���ϸ�'
group by UNDERTAKE_LABORATORYID
) tempA 
group by UNDERTAKE_LABORATORYID
