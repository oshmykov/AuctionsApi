using System;
using System.Linq;
using System.Linq.Expressions;

namespace AuctionsApi.Models.Business.Abstract
{
    public abstract class Specification<T>
    {
        protected bool isInverted = false;

        public abstract Expression<Func<T, bool>> ToExpression();

        public Specification<T> Invert()
        {
            isInverted = true;
            return this;
        }

        public Specification<T> And(Specification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public Specification<T> Or(Specification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public Specification<T> Not(Specification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }
    }

    public class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> left;
        private readonly Specification<T> right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExp = left.ToExpression();
            var rightExp = right.ToExpression();

            BinaryExpression andExp = Expression.AndAlso(leftExp.Body, rightExp.Body);

            return Expression.Lambda<Func<T, bool>>(andExp, leftExp.Parameters.Single());
        }
    }

    public class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> left;
        private readonly Specification<T> right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> leftExp = left.ToExpression();
            Expression<Func<T, bool>> rightExp = right.ToExpression();

            BinaryExpression andExpression = Expression.OrElse(leftExp.Body, rightExp.Body);

            return Expression.Lambda<Func<T, bool>>(andExpression, leftExp.Parameters.Single());
        }
    }

    public class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> invertedPecification;

        public NotSpecification(Specification<T> specification)
        {
            invertedPecification = specification.Invert();
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return invertedPecification.ToExpression();
        }
    }
}
